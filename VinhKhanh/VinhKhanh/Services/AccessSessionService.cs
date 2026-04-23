using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using VinhKhanh.Data;
using VinhKhanh.Dtos;

namespace VinhKhanh.Services
{
    public class AccessSessionService
    {
        private const string PackagePortalDeviceCode = "DEVICE-PACKAGE-PORTAL";
        private const string PackagePortalActivationCode = "ACT-PACKAGE-PORTAL";
        private const string AppClientDevicePrefix = "APP-CLIENT-";
        private const string AppClientActivationPrefix = "ACT-APP-CLIENT-";

        private readonly MySqlDbContext _db;
        private readonly PackageAccessEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly VietQrPayloadBuilder _vietQrPayloadBuilder;

        public AccessSessionService(
            MySqlDbContext db,
            PackageAccessEmailService emailService,
            IConfiguration configuration,
            VietQrPayloadBuilder vietQrPayloadBuilder)
        {
            _db = db;
            _emailService = emailService;
            _configuration = configuration;
            _vietQrPayloadBuilder = vietQrPayloadBuilder;
        }

        public async Task<List<PublicServicePackageDto>> GetActiveServicePackagesAsync()
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT idGoi, ten, moTa, gia, thoiHanNgay
                FROM goidichvu
                WHERE trangThai = 'hoat_dong'
                ORDER BY idGoi;";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            var packages = new List<PublicServicePackageDto>();
            while (await reader.ReadAsync())
            {
                packages.Add(new PublicServicePackageDto
                {
                    IdGoi = reader.GetInt32("idGoi"),
                    Ten = reader["ten"]?.ToString() ?? string.Empty,
                    MoTa = reader["moTa"] == DBNull.Value ? null : reader["moTa"]?.ToString(),
                    Gia = reader["gia"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["gia"]),
                    ThoiHanNgay = reader["thoiHanNgay"] == DBNull.Value ? 0 : Convert.ToInt32(reader["thoiHanNgay"])
                });
            }

            return packages;
        }

        public async Task<AccessSessionResponseDto> CreateFromQrAsync(ScanQrRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.MaThietBi))
            {
                return new AccessSessionResponseDto
                {
                    Success = false,
                    Message = "Thiếu mã thiết bị."
                };
            }

            var maThietBi = request.MaThietBi.Trim();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string selectDeviceSql = @"
                SELECT idThietBi, maThietBi, daKichHoat, trangThai
                FROM thietbi
                WHERE maThietBi = @maThietBi
                LIMIT 1;";

            using var selectDeviceCmd = new MySqlCommand(selectDeviceSql, conn);
            selectDeviceCmd.Parameters.AddWithValue("@maThietBi", maThietBi);

            using var reader = await selectDeviceCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return new AccessSessionResponseDto
                {
                    Success = false,
                    Message = "Không tìm thấy thiết bị."
                };
            }

            var idThietBi = reader.GetInt32("idThietBi");
            var resolvedDeviceCode = reader["maThietBi"]?.ToString() ?? maThietBi;
            var daKichHoat = Convert.ToBoolean(reader["daKichHoat"]);
            var trangThaiThietBi = reader["trangThai"]?.ToString();
            await reader.CloseAsync();

            if (!daKichHoat || !string.Equals(trangThaiThietBi, "hoat_dong", StringComparison.OrdinalIgnoreCase))
            {
                return new AccessSessionResponseDto
                {
                    Success = false,
                    Message = "Thiết bị chưa được kích hoạt hoặc đang không hoạt động.",
                    MaThietBi = resolvedDeviceCode
                };
            }

            var package = await ResolvePackageAsync(conn, resolvedDeviceCode, request.IdGoi);
            if (package is null)
            {
                return new AccessSessionResponseDto
                {
                    Success = false,
                    Message = "Thiáº¿t bá»‹ chÆ°a cÃ³ gÃ³i Ä‘Äƒng kÃ½ hoáº·c gÃ³i khÃ´ng há»£p lá»‡.",
                    MaThietBi = resolvedDeviceCode
                };
            }

            const string expireOldSessionsSql = @"
                UPDATE phien_vao_app
                SET trangThai = 'het_han'
                WHERE maThietBi = @maThietBi
                  AND trangThai = 'hieu_luc';";

            using (var expireCmd = new MySqlCommand(expireOldSessionsSql, conn))
            {
                expireCmd.Parameters.AddWithValue("@maThietBi", resolvedDeviceCode);
                await expireCmd.ExecuteNonQueryAsync();
            }

            var batDauLuc = DateTime.UtcNow;
            var hetHanLuc = batDauLuc.AddDays(package.DurationDays);
            var accessToken = GenerateAccessToken();

            const string insertSql = @"
                INSERT INTO phien_vao_app (idThietBi, maThietBi, idGoi, qrRaw, accessToken, batDauLuc, hetHanLuc, trangThai)
                VALUES (@idThietBi, @maThietBi, @idGoi, @qrRaw, @accessToken, @batDauLuc, @hetHanLuc, 'hieu_luc');";

            using (var insertCmd = new MySqlCommand(insertSql, conn))
            {
                insertCmd.Parameters.AddWithValue("@idThietBi", idThietBi);
                insertCmd.Parameters.AddWithValue("@maThietBi", resolvedDeviceCode);
                insertCmd.Parameters.AddWithValue("@idGoi", package.IdGoi);
                insertCmd.Parameters.AddWithValue("@qrRaw", string.IsNullOrWhiteSpace(request.QrRaw) ? DBNull.Value : request.QrRaw);
                insertCmd.Parameters.AddWithValue("@accessToken", accessToken);
                insertCmd.Parameters.AddWithValue("@batDauLuc", batDauLuc);
                insertCmd.Parameters.AddWithValue("@hetHanLuc", hetHanLuc);
                await insertCmd.ExecuteNonQueryAsync();
            }

            const string updateDeviceSql = @"
                UPDATE thietbi
                SET lanCuoiHoatDong = NOW()
                WHERE idThietBi = @idThietBi;";

            using (var updateDeviceCmd = new MySqlCommand(updateDeviceSql, conn))
            {
                updateDeviceCmd.Parameters.AddWithValue("@idThietBi", idThietBi);
                await updateDeviceCmd.ExecuteNonQueryAsync();
            }

            return new AccessSessionResponseDto
            {
                Success = true,
                Message = "Tạo phiên vào app thành công.",
                MaThietBi = resolvedDeviceCode,
                AccessToken = accessToken,
                BatDauLuc = batDauLuc,
                HetHanLuc = hetHanLuc,
                TrangThai = "hieu_luc",
                IdGoi = package.IdGoi,
                TenGoi = package.TenGoi,
                SoNgayHieuLuc = package.DurationDays
            };
        }

        public async Task<ValidateAccessResponseDto> ValidateAsync(string accessToken, string? clientDeviceId = null)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new ValidateAccessResponseDto
                {
                    IsValid = false,
                    Message = "Thiếu access token."
                };
            }

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT id, idThietBi, maThietBi, batDauLuc, hetHanLuc, trangThai
                FROM phien_vao_app
                WHERE accessToken = @accessToken
                LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@accessToken", accessToken);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return new ValidateAccessResponseDto
                {
                    IsValid = false,
                    Message = "Không tìm thấy phiên truy cập."
                };
            }

            var sessionId = Convert.ToInt64(reader["id"]);
            var idThietBi = reader["idThietBi"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idThietBi"]);
            var maThietBi = reader["maThietBi"]?.ToString();
            var batDauLuc = Convert.ToDateTime(reader["batDauLuc"]);
            var hetHanLuc = Convert.ToDateTime(reader["hetHanLuc"]);
            var trangThai = reader["trangThai"]?.ToString() ?? "het_han";

            await reader.CloseAsync();

            if (trangThai == "hieu_luc" && hetHanLuc <= DateTime.UtcNow)
            {
                const string expireSql = @"
                    UPDATE phien_vao_app
                    SET trangThai = 'het_han'
                    WHERE id = @id
                      AND trangThai = 'hieu_luc';";

                using var expireCmd = new MySqlCommand(expireSql, conn);
                expireCmd.Parameters.AddWithValue("@id", sessionId);
                await expireCmd.ExecuteNonQueryAsync();
                trangThai = "het_han";
            }

            var normalizedClientDeviceId = NormalizeClientDeviceId(clientDeviceId);
            if (trangThai == "hieu_luc" &&
                hetHanLuc > DateTime.UtcNow &&
                string.IsNullOrWhiteSpace(maThietBi))
            {
                return new ValidateAccessResponseDto
                {
                    IsValid = false,
                        Message = "Token chua duoc gan voi thiet bi nao.",
                    MaThietBi = maThietBi,
                    BatDauLuc = batDauLuc,
                    HetHanLuc = hetHanLuc,
                    TrangThai = trangThai
                };
            }

            if (trangThai == "hieu_luc" &&
                hetHanLuc > DateTime.UtcNow &&
                IsClientManagedDeviceCode(maThietBi))
            {
                if (string.IsNullOrWhiteSpace(normalizedClientDeviceId))
                {
                    return new ValidateAccessResponseDto
                    {
                        IsValid = false,
                        Message = "Thieu ma thiet bi client de validate token.",
                        MaThietBi = maThietBi,
                        BatDauLuc = batDauLuc,
                        HetHanLuc = hetHanLuc,
                        TrangThai = trangThai
                    };
                }

                if (!string.Equals(maThietBi, normalizedClientDeviceId, StringComparison.OrdinalIgnoreCase))
                {
                    return new ValidateAccessResponseDto
                    {
                        IsValid = false,
                        Message = "Token da duoc bind voi thiet bi khac. Token nay chi hop le tren thiet bi da dang ky ban dau.",
                        MaThietBi = maThietBi,
                        BatDauLuc = batDauLuc,
                        HetHanLuc = hetHanLuc,
                        TrangThai = "huy"
                    };
                }
            }

            if (idThietBi.HasValue && trangThai == "hieu_luc" && hetHanLuc > DateTime.UtcNow)
                await TouchDeviceAsync(conn, idThietBi.Value);

            return new ValidateAccessResponseDto
            {
                IsValid = trangThai == "hieu_luc" && hetHanLuc > DateTime.UtcNow,
                Message = trangThai == "hieu_luc" && hetHanLuc > DateTime.UtcNow
                    ? "Phiên truy cập còn hiệu lực."
                    : "Phiên truy cập đã hết hạn hoặc không hợp lệ.",
                MaThietBi = maThietBi,
                BatDauLuc = batDauLuc,
                HetHanLuc = hetHanLuc,
                TrangThai = trangThai
            };
        }

        public async Task<RegisterPackageAccessResponseDto> RegisterPackageAccessAsync(RegisterPackageAccessRequestDto request)
        {
            var email = request.Email?.Trim() ?? string.Empty;
            var hasEmail = !string.IsNullOrWhiteSpace(email);
            if (request.SendEmail && (!hasEmail || !email.Contains('@') || !email.Contains('.')))
            {
                return new RegisterPackageAccessResponseDto
                {
                    Success = false,
                    Message = "Email khong hop le."
                };
            }

            if (hasEmail && (!email.Contains('@') || !email.Contains('.')))
            {
                return new RegisterPackageAccessResponseDto
                {
                    Success = false,
                    Message = "Email khong hop le."
                };
            }

            if (!request.BypassPayment)
            {
                return new RegisterPackageAccessResponseDto
                {
                    Success = false,
                    Message = "Luong thanh toan QR that chua duoc implement. Hay bat bypass de test."
                };
            }

            var clientDeviceId = NormalizeClientDeviceId(request.ClientDeviceId);
            if (!IsClientManagedDeviceCode(clientDeviceId))
            {
                return new RegisterPackageAccessResponseDto
                {
                    Success = false,
                    Message = "Client device id khong hop le.",
                    Email = email
                };
            }

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var package = await ResolvePackageAsync(conn, PackagePortalDeviceCode, request.IdGoi);
            if (package is null)
            {
                return new RegisterPackageAccessResponseDto
                {
                    Success = false,
                    Message = "Khong tim thay goi dich vu hop le.",
                    Email = email
                };
            }

            var batDauLuc = DateTime.UtcNow;
            var hetHanLuc = batDauLuc.AddDays(package.DurationDays);
            var accessToken = GenerateAccessToken();
            var qrTokenPayload = $"vkaccess://login?token={accessToken}";
            var deviceId = await EnsureClientDeviceAsync(conn, clientDeviceId);

            await ExpireActiveClientSessionsForDeviceAsync(conn, clientDeviceId);

            const string insertSessionSql = @"
                INSERT INTO phien_vao_app (idThietBi, maThietBi, idGoi, qrRaw, accessToken, batDauLuc, hetHanLuc, trangThai)
                VALUES (@idThietBi, @maThietBi, @idGoi, @qrRaw, @accessToken, @batDauLuc, @hetHanLuc, 'hieu_luc');
                SELECT LAST_INSERT_ID();";

            long sessionId;
            using (var sessionCmd = new MySqlCommand(insertSessionSql, conn))
            {
                sessionCmd.Parameters.AddWithValue("@idThietBi", deviceId);
                sessionCmd.Parameters.AddWithValue("@maThietBi", clientDeviceId);
                sessionCmd.Parameters.AddWithValue("@idGoi", package.IdGoi);
                sessionCmd.Parameters.AddWithValue("@qrRaw", qrTokenPayload);
                sessionCmd.Parameters.AddWithValue("@accessToken", accessToken);
                sessionCmd.Parameters.AddWithValue("@batDauLuc", batDauLuc);
                sessionCmd.Parameters.AddWithValue("@hetHanLuc", hetHanLuc);
                sessionId = Convert.ToInt64(await sessionCmd.ExecuteScalarAsync());
            }

            int invoiceId;
            const string insertInvoiceSql = @"
                INSERT INTO hoadon (idKhachHang, idPhienVaoApp, idGoi, email, tongTien, thoiGianTao, tinhTrang, ghiChu)
                VALUES (NULL, @idPhienVaoApp, @idGoi, @email, @tongTien, NOW(), 'da_thanh_toan', @ghiChu);
                SELECT LAST_INSERT_ID();";

            using (var invoiceCmd = new MySqlCommand(insertInvoiceSql, conn))
            {
                invoiceCmd.Parameters.AddWithValue("@idPhienVaoApp", sessionId);
                invoiceCmd.Parameters.AddWithValue("@idGoi", package.IdGoi);
                invoiceCmd.Parameters.AddWithValue("@email", hasEmail ? (object)email : DBNull.Value);
                invoiceCmd.Parameters.AddWithValue("@tongTien", package.Price);
                invoiceCmd.Parameters.AddWithValue("@ghiChu", "Bypass thanh toan QR de test package access.");
                invoiceId = Convert.ToInt32(await invoiceCmd.ExecuteScalarAsync());
            }

            await TouchDeviceAsync(conn, deviceId);

            var emailResult = request.SendEmail && hasEmail
                ? await _emailService.TrySendQrTokenEmailAsync(
                    email,
                    package.TenGoi,
                    accessToken,
                    qrTokenPayload,
                    hetHanLuc)
                : (Sent: false, Message: "Nguoi dung chon tai QR ve may thay vi nhan email.");

            return new RegisterPackageAccessResponseDto
            {
                Success = true,
                Message = "Dang ky goi, kich hoat token tren may hien tai va sinh QR token dang nhap thanh cong.",
                Email = email,
                MaThietBi = clientDeviceId,
                IdGoi = package.IdGoi,
                TenGoi = package.TenGoi,
                SoNgayHieuLuc = package.DurationDays,
                AccessToken = accessToken,
                BatDauLuc = batDauLuc,
                HetHanLuc = hetHanLuc,
                TrangThai = "hieu_luc",
                QrTokenPayload = qrTokenPayload,
                EmailSent = emailResult.Sent,
                EmailStatusMessage = emailResult.Message,
                IdHoaDon = invoiceId
            };
        }

        public async Task<PackagePaymentResponseDto> CreatePackagePaymentAsync(CreatePackagePaymentRequestDto request)
        {
            var email = request.Email?.Trim() ?? string.Empty;
            var hasEmail = !string.IsNullOrWhiteSpace(email);

            if (hasEmail && (!email.Contains('@') || !email.Contains('.')))
            {
                return new PackagePaymentResponseDto
                {
                    Success = false,
                    Message = "Email khong hop le."
                };
            }

            var clientDeviceId = NormalizeClientDeviceId(request.ClientDeviceId);
            if (!IsClientManagedDeviceCode(clientDeviceId))
            {
                return new PackagePaymentResponseDto
                {
                    Success = false,
                    Message = "Client device id khong hop le.",
                    Email = email
                };
            }

            var bankBin = _configuration["Payment:VietQr:BankBin"] ?? string.Empty;
            var bankAccountNo = _configuration["Payment:VietQr:BankAccountNo"] ?? string.Empty;
            var bankAccountName = _configuration["Payment:VietQr:BankAccountName"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(bankBin) ||
                string.IsNullOrWhiteSpace(bankAccountNo) ||
                string.IsNullOrWhiteSpace(bankAccountName))
            {
                return new PackagePaymentResponseDto
                {
                    Success = false,
                    Message = "Chua cau hinh Payment:VietQr trong appsettings."
                };
            }

            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            await EnsurePaymentColumnsAsync(conn);

            var package = await ResolvePackageAsync(conn, PackagePortalDeviceCode, request.IdGoi);
            if (package is null)
            {
                return new PackagePaymentResponseDto
                {
                    Success = false,
                    Message = "Khong tim thay goi dich vu hop le.",
                    Email = email
                };
            }

            await EnsureClientDeviceAsync(conn, clientDeviceId);

            const string insertInvoiceSql = @"
                INSERT INTO hoadon (idKhachHang, idPhienVaoApp, idGoi, email, tongTien, thoiGianTao, tinhTrang, ghiChu, maThietBi, guiEmail)
                VALUES (NULL, NULL, @idGoi, @email, @tongTien, NOW(), 'moi_tao', @ghiChu, @maThietBi, @guiEmail);
                SELECT LAST_INSERT_ID();";

            int invoiceId;
            using (var invoiceCmd = new MySqlCommand(insertInvoiceSql, conn))
            {
                invoiceCmd.Parameters.AddWithValue("@idGoi", package.IdGoi);
                invoiceCmd.Parameters.AddWithValue("@email", hasEmail ? (object)email : DBNull.Value);
                invoiceCmd.Parameters.AddWithValue("@tongTien", package.Price);
                invoiceCmd.Parameters.AddWithValue("@ghiChu", "Cho thanh toan VietQR/Casso.");
                invoiceCmd.Parameters.AddWithValue("@maThietBi", clientDeviceId);
                invoiceCmd.Parameters.AddWithValue("@guiEmail", request.SendEmail && hasEmail);
                invoiceId = Convert.ToInt32(await invoiceCmd.ExecuteScalarAsync());
            }

            var paymentReference = $"CSAT{invoiceId}";
            var paymentContent = VietQrPayloadBuilder.NormalizePaymentContent(invoiceId, package.IdGoi, clientDeviceId);
            var paymentQrPayload = _vietQrPayloadBuilder.BuildPayload(
                bankBin,
                bankAccountNo,
                bankAccountName,
                package.Price,
                paymentContent);

            const string updateInvoiceSql = @"
                UPDATE hoadon
                SET maThanhToan = @maThanhToan,
                    ghiChu = @ghiChu
                WHERE idHoaDon = @idHoaDon;";

            using (var updateCmd = new MySqlCommand(updateInvoiceSql, conn))
            {
                updateCmd.Parameters.AddWithValue("@maThanhToan", paymentReference);
                updateCmd.Parameters.AddWithValue("@ghiChu", $"VietQR {paymentContent}; goi={package.IdGoi}; thietBi={clientDeviceId}");
                updateCmd.Parameters.AddWithValue("@idHoaDon", invoiceId);
                await updateCmd.ExecuteNonQueryAsync();
            }

            return new PackagePaymentResponseDto
            {
                Success = true,
                Message = "Da tao ma QR thanh toan VietQR. Cho Casso webhook xac nhan giao dich.",
                Email = email,
                MaThietBi = clientDeviceId,
                IdGoi = package.IdGoi,
                TenGoi = package.TenGoi,
                SoNgayHieuLuc = package.DurationDays,
                IdHoaDon = invoiceId,
                Amount = package.Price,
                PaymentReference = paymentReference,
                PaymentContent = paymentContent,
                PaymentQrPayload = paymentQrPayload,
                BankBin = bankBin,
                BankAccountNo = bankAccountNo,
                BankAccountName = bankAccountName,
                PaymentCreatedAt = DateTime.UtcNow,
                PaymentStatus = "moi_tao"
            };
        }

        public async Task<PackagePaymentResponseDto> GetPackagePaymentStatusAsync(string paymentReference)
        {
            paymentReference = NormalizePaymentReference(paymentReference);
            if (string.IsNullOrWhiteSpace(paymentReference))
            {
                return new PackagePaymentResponseDto
                {
                    Success = false,
                    Message = "Thieu ma thanh toan."
                };
            }

            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            await EnsurePaymentColumnsAsync(conn);

            return await BuildPaymentStatusResponseAsync(conn, paymentReference);
        }

        public bool IsValidCassoSecurityKey(string? providedKey)
        {
            var configuredKey = _configuration["Payment:Casso:WebhookSecurityKey"] ?? string.Empty;
            if (string.IsNullOrWhiteSpace(configuredKey))
                return true;

            return !string.IsNullOrWhiteSpace(providedKey) &&
                   CryptographicOperations.FixedTimeEquals(
                       System.Text.Encoding.UTF8.GetBytes(configuredKey),
                       System.Text.Encoding.UTF8.GetBytes(providedKey));
        }

        public async Task<CassoWebhookProcessResultDto> HandleCassoWebhookAsync(JsonElement payload)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            await EnsurePaymentColumnsAsync(conn);

            var transactions = ExtractCassoTransactions(payload);
            var result = new CassoWebhookProcessResultDto
            {
                Success = true,
                Message = "Da xu ly Casso webhook."
            };

            foreach (var transaction in transactions)
            {
                result.Processed++;

                if (transaction.Amount <= 0 || string.IsNullOrWhiteSpace(transaction.Description))
                {
                    result.Ignored++;
                    continue;
                }

                var packagePaymentReference = ExtractPaymentReference(transaction.Description);
                if (!string.IsNullOrWhiteSpace(packagePaymentReference))
                {
                    var activateResult = await ActivatePaidPackageInvoiceAsync(
                        conn,
                        packagePaymentReference,
                        transaction.Amount,
                        transaction.TransactionId,
                        transaction.PaidAt,
                        transaction.Description);

                    if (activateResult.Success && !string.IsNullOrWhiteSpace(activateResult.AccessToken))
                        result.Activated++;
                    else
                        result.Ignored++;
                        
                    continue;
                }

                var storeInvoiceReference = ExtractStoreInvoiceReference(transaction.Description);
                if (!string.IsNullOrWhiteSpace(storeInvoiceReference))
                {
                    var invoiceIdStr = storeInvoiceReference.Substring(4); // Remove HDGH
                    if (int.TryParse(invoiceIdStr, out var invoiceId))
                    {
                        var activateStoreResult = await ActivateStoreInvoiceAsync(
                            conn,
                            invoiceId,
                            transaction.Amount);
                            
                        if (activateStoreResult)
                            result.Activated++;
                        else
                            result.Ignored++;
                            
                        continue;
                    }
                }

                result.Ignored++;
            }

            return result;
        }

        public async Task<AccessSessionResponseDto> ActivateTokenAsync(ActivateAccessTokenRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.AccessToken))
            {
                return new AccessSessionResponseDto
                {
                    Success = false,
                    Message = "Thieu access token de kich hoat."
                };
            }

            var clientDeviceId = NormalizeClientDeviceId(request.ClientDeviceId);
            if (!IsClientManagedDeviceCode(clientDeviceId))
            {
                return new AccessSessionResponseDto
                {
                    Success = false,
                    Message = "Client device id khong hop le."
                };
            }

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT pva.id, pva.idThietBi, pva.maThietBi, pva.idGoi, pva.accessToken, pva.batDauLuc, pva.hetHanLuc, pva.trangThai,
                       gdv.ten AS tenGoi, gdv.thoiHanNgay
                FROM phien_vao_app pva
                LEFT JOIN goidichvu gdv ON gdv.idGoi = pva.idGoi
                WHERE pva.accessToken = @accessToken
                LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@accessToken", request.AccessToken.Trim());

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return new AccessSessionResponseDto
                {
                    Success = false,
                    Message = "Khong tim thay token truy cap."
                };
            }

            var sessionId = Convert.ToInt64(reader["id"]);
            var currentDeviceCode = reader["maThietBi"]?.ToString() ?? string.Empty;
            var idGoi = reader["idGoi"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idGoi"]);
            var batDauLuc = Convert.ToDateTime(reader["batDauLuc"]);
            var hetHanLuc = Convert.ToDateTime(reader["hetHanLuc"]);
            var trangThai = reader["trangThai"]?.ToString() ?? "het_han";
            var tenGoi = reader["tenGoi"]?.ToString();
            var soNgayHieuLuc = reader["thoiHanNgay"] == DBNull.Value ? (int?)null : NormalizeDurationDays(reader["thoiHanNgay"]);
            await reader.CloseAsync();

            if (trangThai == "hieu_luc" && hetHanLuc <= DateTime.UtcNow)
            {
                const string expireSql = @"
                    UPDATE phien_vao_app
                    SET trangThai = 'het_han'
                    WHERE id = @id
                      AND trangThai = 'hieu_luc';";

                using var expireCmd = new MySqlCommand(expireSql, conn);
                expireCmd.Parameters.AddWithValue("@id", sessionId);
                await expireCmd.ExecuteNonQueryAsync();

                return new AccessSessionResponseDto
                {
                    Success = false,
                    Message = "Token da het han, khong the kich hoat.",
                    MaThietBi = currentDeviceCode,
                    BatDauLuc = batDauLuc,
                    HetHanLuc = hetHanLuc,
                    TrangThai = "het_han",
                    IdGoi = idGoi,
                    TenGoi = tenGoi,
                    SoNgayHieuLuc = soNgayHieuLuc
                };
            }

            if (!string.Equals(trangThai, "hieu_luc", StringComparison.OrdinalIgnoreCase))
            {
                return new AccessSessionResponseDto
                {
                    Success = false,
                    Message = "Token khong con hieu luc.",
                    MaThietBi = currentDeviceCode,
                    BatDauLuc = batDauLuc,
                    HetHanLuc = hetHanLuc,
                    TrangThai = trangThai,
                    IdGoi = idGoi,
                    TenGoi = tenGoi,
                    SoNgayHieuLuc = soNgayHieuLuc
                };
            }

            if (string.IsNullOrWhiteSpace(currentDeviceCode))
            {
                return new AccessSessionResponseDto
                {
                    Success = false,
                    Message = "Token chua duoc gan voi thiet bi nao.",
                    BatDauLuc = batDauLuc,
                    HetHanLuc = hetHanLuc,
                    TrangThai = trangThai,
                    IdGoi = idGoi,
                    TenGoi = tenGoi,
                    SoNgayHieuLuc = soNgayHieuLuc
                };
            }

            if (!string.Equals(currentDeviceCode, clientDeviceId, StringComparison.OrdinalIgnoreCase))
            {
                return new AccessSessionResponseDto
                {
                    Success = false,
                    Message = "Token da duoc khoa vao thiet bi khac. Token nay chi dung duoc tren mot may.",
                    MaThietBi = currentDeviceCode,
                    BatDauLuc = batDauLuc,
                    HetHanLuc = hetHanLuc,
                    TrangThai = trangThai,
                    IdGoi = idGoi,
                    TenGoi = tenGoi,
                    SoNgayHieuLuc = soNgayHieuLuc
                };
            }

            var targetDeviceId = await EnsureClientDeviceAsync(conn, clientDeviceId);
            await ExpireActiveClientSessionsForDeviceAsync(conn, clientDeviceId, request.AccessToken.Trim());

            await TouchDeviceAsync(conn, targetDeviceId);

            return new AccessSessionResponseDto
            {
                Success = true,
                Message = "Token hop le tren thiet bi hien tai.",
                MaThietBi = clientDeviceId,
                AccessToken = request.AccessToken.Trim(),
                BatDauLuc = batDauLuc,
                HetHanLuc = hetHanLuc,
                TrangThai = "hieu_luc",
                IdGoi = idGoi,
                TenGoi = tenGoi,
                SoNgayHieuLuc = soNgayHieuLuc
            };
        }

        private async Task<PackagePaymentResponseDto> BuildPaymentStatusResponseAsync(MySqlConnection conn, string paymentReference, MySqlTransaction? transaction = null)
        {
            const string sql = @"
                SELECT hd.idHoaDon, hd.idPhienVaoApp, hd.idGoi, hd.email, hd.tongTien, hd.thoiGianTao, hd.tinhTrang,
                       hd.ghiChu, hd.maThanhToan, hd.maThietBi, hd.guiEmail, hd.thoiGianThanhToan, hd.cassoTransactionId,
                       gdv.ten AS tenGoi, gdv.thoiHanNgay,
                       pva.accessToken, pva.qrRaw, pva.batDauLuc, pva.hetHanLuc, pva.trangThai AS trangThaiPhien
                FROM hoadon hd
                LEFT JOIN goidichvu gdv ON gdv.idGoi = hd.idGoi
                LEFT JOIN phien_vao_app pva ON pva.id = hd.idPhienVaoApp
                WHERE hd.maThanhToan = @maThanhToan
                LIMIT 1;";

            using var cmd = CreateCommand(sql, conn, transaction);
            cmd.Parameters.AddWithValue("@maThanhToan", paymentReference);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return new PackagePaymentResponseDto
                {
                    Success = false,
                    Message = "Khong tim thay yeu cau thanh toan."
                };
            }

            var status = reader["tinhTrang"]?.ToString() ?? "moi_tao";
            var accessToken = reader["accessToken"]?.ToString();
            var qrRaw = reader["qrRaw"]?.ToString();

            return new PackagePaymentResponseDto
            {
                Success = true,
                Message = string.Equals(status, "da_thanh_toan", StringComparison.OrdinalIgnoreCase) &&
                          !string.IsNullOrWhiteSpace(accessToken)
                    ? "Thanh toan da duoc xac nhan va QR token da san sang."
                    : "Chua nhan duoc xac nhan thanh toan tu Casso.",
                Email = reader["email"]?.ToString() ?? string.Empty,
                MaThietBi = reader["maThietBi"]?.ToString(),
                IdGoi = reader["idGoi"] == DBNull.Value ? null : Convert.ToInt32(reader["idGoi"]),
                TenGoi = reader["tenGoi"]?.ToString(),
                SoNgayHieuLuc = reader["thoiHanNgay"] == DBNull.Value ? null : NormalizeDurationDays(reader["thoiHanNgay"]),
                IdHoaDon = Convert.ToInt32(reader["idHoaDon"]),
                Amount = reader["tongTien"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tongTien"]),
                PaymentReference = reader["maThanhToan"]?.ToString(),
                PaymentContent = reader["ghiChu"]?.ToString(),
                PaymentStatus = status,
                PaymentCreatedAt = reader["thoiGianTao"] == DBNull.Value ? null : Convert.ToDateTime(reader["thoiGianTao"]),
                PaymentPaidAt = reader["thoiGianThanhToan"] == DBNull.Value ? null : Convert.ToDateTime(reader["thoiGianThanhToan"]),
                AccessToken = accessToken,
                QrTokenPayload = qrRaw,
                BatDauLuc = reader["batDauLuc"] == DBNull.Value ? null : Convert.ToDateTime(reader["batDauLuc"]),
                HetHanLuc = reader["hetHanLuc"] == DBNull.Value ? null : Convert.ToDateTime(reader["hetHanLuc"]),
                TrangThai = reader["trangThaiPhien"]?.ToString(),
                EmailSent = false,
                EmailStatusMessage = string.Empty
            };
        }

        private async Task<RegisterPackageAccessResponseDto> ActivatePaidPackageInvoiceAsync(
            MySqlConnection conn,
            string paymentReference,
            decimal paidAmount,
            string? transactionId,
            DateTime? paidAt,
            string transactionDescription)
        {
            await using var transaction = await conn.BeginTransactionAsync();

            try
            {
                var result = await ActivatePaidPackageInvoiceLockedAsync(
                    conn,
                    transaction,
                    paymentReference,
                    paidAmount,
                    transactionId,
                    paidAt,
                    transactionDescription);

                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task<RegisterPackageAccessResponseDto> ActivatePaidPackageInvoiceLockedAsync(
            MySqlConnection conn,
            MySqlTransaction transaction,
            string paymentReference,
            decimal paidAmount,
            string? transactionId,
            DateTime? paidAt,
            string transactionDescription)
        {
            if (!string.IsNullOrWhiteSpace(transactionId))
            {
                const string duplicateSql = @"
                    SELECT maThanhToan
                    FROM hoadon
                    WHERE cassoTransactionId = @cassoTransactionId
                      AND cassoTransactionId IS NOT NULL
                      AND cassoTransactionId <> ''
                    LIMIT 1
                    FOR UPDATE;";

                using var duplicateCmd = CreateCommand(duplicateSql, conn, transaction);
                duplicateCmd.Parameters.AddWithValue("@cassoTransactionId", transactionId);
                var existingRef = await duplicateCmd.ExecuteScalarAsync();
                if (existingRef != null && existingRef != DBNull.Value &&
                    !string.Equals(existingRef.ToString(), paymentReference, StringComparison.OrdinalIgnoreCase))
                {
                    return new RegisterPackageAccessResponseDto
                    {
                        Success = false,
                        Message = "Giao dich Casso da duoc gan cho hoa don khac."
                    };
                }
            }

            const string invoiceSql = @"
                SELECT idHoaDon, idPhienVaoApp, idGoi, email, tongTien, tinhTrang, maThietBi, guiEmail
                FROM hoadon
                WHERE maThanhToan = @maThanhToan
                LIMIT 1
                FOR UPDATE;";

            int invoiceId;
            long? existingSessionId;
            int packageId;
            string email;
            decimal invoiceAmount;
            string invoiceStatus;
            string clientDeviceId;
            bool sendEmail;

            using (var invoiceCmd = CreateCommand(invoiceSql, conn, transaction))
            {
                invoiceCmd.Parameters.AddWithValue("@maThanhToan", paymentReference);
                using var reader = await invoiceCmd.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    return new RegisterPackageAccessResponseDto
                    {
                        Success = false,
                        Message = "Khong tim thay hoa don theo noi dung chuyen khoan."
                    };
                }

                invoiceId = Convert.ToInt32(reader["idHoaDon"]);
                existingSessionId = reader["idPhienVaoApp"] == DBNull.Value ? null : Convert.ToInt64(reader["idPhienVaoApp"]);
                packageId = reader["idGoi"] == DBNull.Value ? 0 : Convert.ToInt32(reader["idGoi"]);
                email = reader["email"]?.ToString() ?? string.Empty;
                invoiceAmount = reader["tongTien"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tongTien"]);
                invoiceStatus = reader["tinhTrang"]?.ToString() ?? "moi_tao";
                clientDeviceId = NormalizeClientDeviceId(reader["maThietBi"]?.ToString());
                sendEmail = reader["guiEmail"] != DBNull.Value && Convert.ToBoolean(reader["guiEmail"]);
            }

            if (paidAmount < invoiceAmount)
            {
                return new RegisterPackageAccessResponseDto
                {
                    Success = false,
                    Message = "So tien giao dich nho hon so tien goi."
                };
            }

            if (string.Equals(invoiceStatus, "da_thanh_toan", StringComparison.OrdinalIgnoreCase) &&
                existingSessionId.HasValue)
            {
                var paidStatus = await BuildPaymentStatusResponseAsync(conn, paymentReference, transaction);
                return new RegisterPackageAccessResponseDto
                {
                    Success = paidStatus.Success,
                    Message = paidStatus.Message,
                    Email = paidStatus.Email,
                    MaThietBi = paidStatus.MaThietBi,
                    IdGoi = paidStatus.IdGoi,
                    TenGoi = paidStatus.TenGoi,
                    SoNgayHieuLuc = paidStatus.SoNgayHieuLuc,
                    AccessToken = paidStatus.AccessToken,
                    BatDauLuc = paidStatus.BatDauLuc,
                    HetHanLuc = paidStatus.HetHanLuc,
                    TrangThai = paidStatus.TrangThai,
                    QrTokenPayload = paidStatus.QrTokenPayload,
                    IdHoaDon = paidStatus.IdHoaDon
                };
            }

            if (!string.Equals(invoiceStatus, "moi_tao", StringComparison.OrdinalIgnoreCase))
            {
                return new RegisterPackageAccessResponseDto
                {
                    Success = false,
                    Message = "Hoa don khong o trang thai cho thanh toan."
                };
            }

            if (packageId <= 0 || !IsClientManagedDeviceCode(clientDeviceId))
            {
                return new RegisterPackageAccessResponseDto
                {
                    Success = false,
                    Message = "Hoa don thieu ma goi hoac ma thiet bi."
                };
            }

            var package = await ResolvePackageAsync(conn, PackagePortalDeviceCode, packageId, transaction);
            if (package is null)
            {
                return new RegisterPackageAccessResponseDto
                {
                    Success = false,
                    Message = "Goi dich vu tren hoa don khong hop le."
                };
            }

            var batDauLuc = DateTime.UtcNow;
            var hetHanLuc = batDauLuc.AddDays(package.DurationDays);
            var accessToken = GenerateAccessToken();
            var qrTokenPayload = $"vkaccess://login?token={accessToken}";
            var deviceId = await EnsureClientDeviceAsync(conn, clientDeviceId, transaction);

            await ExpireActiveClientSessionsForDeviceAsync(conn, clientDeviceId, transaction: transaction);

            const string insertSessionSql = @"
                INSERT INTO phien_vao_app (idThietBi, maThietBi, idGoi, qrRaw, accessToken, batDauLuc, hetHanLuc, trangThai)
                VALUES (@idThietBi, @maThietBi, @idGoi, @qrRaw, @accessToken, @batDauLuc, @hetHanLuc, 'hieu_luc');
                SELECT LAST_INSERT_ID();";

            long sessionId;
            using (var sessionCmd = CreateCommand(insertSessionSql, conn, transaction))
            {
                sessionCmd.Parameters.AddWithValue("@idThietBi", deviceId);
                sessionCmd.Parameters.AddWithValue("@maThietBi", clientDeviceId);
                sessionCmd.Parameters.AddWithValue("@idGoi", package.IdGoi);
                sessionCmd.Parameters.AddWithValue("@qrRaw", qrTokenPayload);
                sessionCmd.Parameters.AddWithValue("@accessToken", accessToken);
                sessionCmd.Parameters.AddWithValue("@batDauLuc", batDauLuc);
                sessionCmd.Parameters.AddWithValue("@hetHanLuc", hetHanLuc);
                sessionId = Convert.ToInt64(await sessionCmd.ExecuteScalarAsync());
            }

            const string updateInvoiceSql = @"
                UPDATE hoadon
                SET idPhienVaoApp = @idPhienVaoApp,
                    tinhTrang = 'da_thanh_toan',
                    thoiGianThanhToan = @thoiGianThanhToan,
                    cassoTransactionId = @cassoTransactionId,
                    ghiChu = @ghiChu
                WHERE idHoaDon = @idHoaDon;";

            using (var updateCmd = CreateCommand(updateInvoiceSql, conn, transaction))
            {
                updateCmd.Parameters.AddWithValue("@idPhienVaoApp", sessionId);
                updateCmd.Parameters.AddWithValue("@thoiGianThanhToan", paidAt ?? DateTime.UtcNow);
                updateCmd.Parameters.AddWithValue("@cassoTransactionId", string.IsNullOrWhiteSpace(transactionId) ? DBNull.Value : transactionId);
                updateCmd.Parameters.AddWithValue("@ghiChu", $"Casso xac nhan: {transactionDescription}");
                updateCmd.Parameters.AddWithValue("@idHoaDon", invoiceId);
                await updateCmd.ExecuteNonQueryAsync();
            }

            await TouchDeviceAsync(conn, deviceId, transaction);

            var hasEmail = !string.IsNullOrWhiteSpace(email);
            var emailResult = sendEmail && hasEmail
                ? await _emailService.TrySendQrTokenEmailAsync(
                    email,
                    package.TenGoi,
                    accessToken,
                    qrTokenPayload,
                    hetHanLuc)
                : (Sent: false, Message: "Nguoi dung chon tai QR ve may thay vi nhan email.");

            return new RegisterPackageAccessResponseDto
            {
                Success = true,
                Message = "Casso da xac nhan thanh toan, kich hoat token va sinh QR token dang nhap thanh cong.",
                Email = email,
                MaThietBi = clientDeviceId,
                IdGoi = package.IdGoi,
                TenGoi = package.TenGoi,
                SoNgayHieuLuc = package.DurationDays,
                AccessToken = accessToken,
                BatDauLuc = batDauLuc,
                HetHanLuc = hetHanLuc,
                TrangThai = "hieu_luc",
                QrTokenPayload = qrTokenPayload,
                EmailSent = emailResult.Sent,
                EmailStatusMessage = emailResult.Message,
                IdHoaDon = invoiceId
            };
        }

        private static IEnumerable<CassoTransaction> ExtractCassoTransactions(JsonElement payload)
        {
            if (payload.ValueKind != JsonValueKind.Object ||
                !payload.TryGetProperty("data", out var data))
            {
                yield break;
            }

            if (data.ValueKind == JsonValueKind.Object &&
                data.TryGetProperty("records", out var records))
            {
                data = records;
            }

            if (data.ValueKind != JsonValueKind.Array)
                yield break;

            foreach (var item in data.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                    continue;

                yield return new CassoTransaction(
                    TransactionId: GetJsonString(item, "id") ?? GetJsonString(item, "tid"),
                    Description: GetJsonString(item, "description") ?? string.Empty,
                    Amount: GetJsonDecimal(item, "amount"),
                    PaidAt: GetJsonDateTime(item, "when") ?? GetJsonDateTime(item, "createdAt"));
            }
        }

        private static string? GetJsonString(JsonElement item, string propertyName)
        {
            if (!item.TryGetProperty(propertyName, out var value))
                return null;

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString(),
                JsonValueKind.Number => value.GetRawText(),
                _ => null
            };
        }

        private static decimal GetJsonDecimal(JsonElement item, string propertyName)
        {
            if (!item.TryGetProperty(propertyName, out var value))
                return 0;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number))
                return number;

            if (value.ValueKind == JsonValueKind.String &&
                decimal.TryParse(value.GetString(), out var parsed))
                return parsed;

            return 0;
        }

        private static DateTime? GetJsonDateTime(JsonElement item, string propertyName)
        {
            if (!item.TryGetProperty(propertyName, out var value))
                return null;

            if (value.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(value.GetString(), out var parsed))
                return parsed;

            return null;
        }

        private static string NormalizePaymentReference(string? value)
        {
            var raw = value?.Trim().ToUpperInvariant() ?? string.Empty;
            var match = Regex.Match(raw, @"CSAT\d+", RegexOptions.IgnoreCase);
            return match.Success ? match.Value.ToUpperInvariant() : raw;
        }

        private static string? ExtractPaymentReference(string description)
        {
            var match = Regex.Match(description ?? string.Empty, @"CSAT\d+", RegexOptions.IgnoreCase);
            return match.Success ? match.Value.ToUpperInvariant() : null;
        }

        private static string? ExtractStoreInvoiceReference(string description)
        {
            var match = Regex.Match(description ?? string.Empty, @"HDGH\d+", RegexOptions.IgnoreCase);
            return match.Success ? match.Value.ToUpperInvariant() : null;
        }

        private static async Task<bool> ActivateStoreInvoiceAsync(MySqlConnection conn, int invoiceId, decimal amount)
        {
            const string sql = @"
                UPDATE hoadongianhang 
                SET trangThai = 'da_thanh_toan' 
                WHERE idHoaDonGianHang = @invoiceId 
                  AND tongTien <= @amount 
                  AND trangThai = 'chua_thanh_toan';";
                  
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@invoiceId", invoiceId);
            cmd.Parameters.AddWithValue("@amount", amount);
            
            var affected = await cmd.ExecuteNonQueryAsync();
            if (affected > 0)
            {
                const string updateReqSql = @"
                    UPDATE yeucaugianhang 
                    SET trangThai = 'da_duyet' 
                    WHERE idGianHang = (SELECT idGianHang FROM hoadongianhang WHERE idHoaDonGianHang = @invoiceId) 
                      AND trangThai = 'cho_thanh_toan';";
                using var reqCmd = new MySqlCommand(updateReqSql, conn);
                reqCmd.Parameters.AddWithValue("@invoiceId", invoiceId);
                await reqCmd.ExecuteNonQueryAsync();

                const string updateStoreSql = @"
                    UPDATE gianhang 
                    SET tinhTrang = 'dang_hoat_dong' 
                    WHERE idGianHang = (SELECT idGianHang FROM hoadongianhang WHERE idHoaDonGianHang = @invoiceId);";
                using var storeCmd = new MySqlCommand(updateStoreSql, conn);
                storeCmd.Parameters.AddWithValue("@invoiceId", invoiceId);
                await storeCmd.ExecuteNonQueryAsync();
                
                return true;
            }
            
            return false;
        }

        private static async Task EnsurePaymentColumnsAsync(MySqlConnection conn)
        {
            var columns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["maThanhToan"] = "ALTER TABLE hoadon ADD COLUMN maThanhToan varchar(32) DEFAULT NULL AFTER idGoi",
                ["maThietBi"] = "ALTER TABLE hoadon ADD COLUMN maThietBi varchar(100) DEFAULT NULL AFTER maThanhToan",
                ["guiEmail"] = "ALTER TABLE hoadon ADD COLUMN guiEmail tinyint(1) NOT NULL DEFAULT 0 AFTER email",
                ["thoiGianThanhToan"] = "ALTER TABLE hoadon ADD COLUMN thoiGianThanhToan datetime DEFAULT NULL AFTER thoiGianTao",
                ["cassoTransactionId"] = "ALTER TABLE hoadon ADD COLUMN cassoTransactionId varchar(80) DEFAULT NULL AFTER tinhTrang"
            };

            const string existingColumnsSql = @"
                SELECT COLUMN_NAME
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'hoadon';";

            var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            using (var cmd = new MySqlCommand(existingColumnsSql, conn))
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                    existing.Add(reader.GetString("COLUMN_NAME"));
            }

            foreach (var column in columns)
            {
                if (existing.Contains(column.Key))
                    continue;

                using var alterCmd = new MySqlCommand(column.Value, conn);
                await alterCmd.ExecuteNonQueryAsync();
            }

            await EnsureIndexAsync(conn, "uq_hoadon_maThanhToan", "CREATE UNIQUE INDEX uq_hoadon_maThanhToan ON hoadon (maThanhToan)");
            await EnsureIndexAsync(conn, "idx_hoadon_maThietBi", "CREATE INDEX idx_hoadon_maThietBi ON hoadon (maThietBi)");
            await EnsureIndexAsync(conn, "idx_hoadon_cassoTransactionId", "CREATE INDEX idx_hoadon_cassoTransactionId ON hoadon (cassoTransactionId)");
        }

        private static async Task EnsureIndexAsync(MySqlConnection conn, string indexName, string createSql)
        {
            const string sql = @"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.STATISTICS
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = 'hoadon'
                  AND INDEX_NAME = @indexName;";

            using (var cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@indexName", indexName);
                var exists = Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
                if (exists)
                    return;
            }

            using var createCmd = new MySqlCommand(createSql, conn);
            await createCmd.ExecuteNonQueryAsync();
        }

        private static async Task<PackageInfo?> ResolvePackageAsync(MySqlConnection conn, string maThietBi, int? requestedPackageId, MySqlTransaction? transaction = null)
        {
            const string explicitPackageSql = @"
                SELECT idGoi, ten, thoiHanNgay, gia
                FROM goidichvu
                WHERE idGoi = @idGoi
                  AND trangThai = 'hoat_dong'
                LIMIT 1;";

            if (requestedPackageId.HasValue)
            {
                using var explicitCmd = CreateCommand(explicitPackageSql, conn, transaction);
                explicitCmd.Parameters.AddWithValue("@idGoi", requestedPackageId.Value);

                using var explicitReader = await explicitCmd.ExecuteReaderAsync();
                if (await explicitReader.ReadAsync())
                {
                    return new PackageInfo(
                        explicitReader.GetInt32("idGoi"),
                        explicitReader["ten"]?.ToString() ?? $"Goi {requestedPackageId.Value}",
                        NormalizeDurationDays(explicitReader["thoiHanNgay"]),
                        explicitReader.GetDecimal("gia"));
                }

                return null;
            }

            const string lastRegisteredPackageSql = @"
                SELECT gdv.idGoi, gdv.ten, gdv.thoiHanNgay, gdv.gia
                FROM phien_vao_app pva
                INNER JOIN goidichvu gdv ON gdv.idGoi = pva.idGoi
                WHERE pva.maThietBi = @maThietBi
                  AND pva.idGoi IS NOT NULL
                  AND gdv.trangThai = 'hoat_dong'
                ORDER BY pva.batDauLuc DESC, pva.id DESC
                LIMIT 1;";

            using var lastRegisteredCmd = CreateCommand(lastRegisteredPackageSql, conn, transaction);
            lastRegisteredCmd.Parameters.AddWithValue("@maThietBi", maThietBi);

            using var lastRegisteredReader = await lastRegisteredCmd.ExecuteReaderAsync();
            if (!await lastRegisteredReader.ReadAsync())
                return null;

            return new PackageInfo(
                lastRegisteredReader.GetInt32("idGoi"),
                lastRegisteredReader["ten"]?.ToString() ?? string.Empty,
                NormalizeDurationDays(lastRegisteredReader["thoiHanNgay"]),
                lastRegisteredReader.GetDecimal("gia"));
        }

        private static async Task<int> EnsurePackagePortalDeviceAsync(MySqlConnection conn)
        {
            const string selectSql = @"
                SELECT idThietBi
                FROM thietbi
                WHERE maThietBi = @maThietBi
                LIMIT 1;";

            using (var selectCmd = new MySqlCommand(selectSql, conn))
            {
                selectCmd.Parameters.AddWithValue("@maThietBi", PackagePortalDeviceCode);
                var existing = await selectCmd.ExecuteScalarAsync();
                if (existing != null && existing != DBNull.Value)
                    return Convert.ToInt32(existing);
            }

            const string insertSql = @"
                INSERT INTO thietbi (maThietBi, maKichHoat, idTaiKhoan, daKichHoat, thoiGianKichHoat, ngayTao, lanCuoiHoatDong, trangThai, loaiThietBi)
                VALUES (@maThietBi, @maKichHoat, NULL, 1, NOW(), NOW(), NOW(), 'hoat_dong', 'portal_web');
                SELECT LAST_INSERT_ID();";

            using var insertCmd = new MySqlCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("@maThietBi", PackagePortalDeviceCode);
            insertCmd.Parameters.AddWithValue("@maKichHoat", PackagePortalActivationCode);
            return Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
        }

        private static async Task<int> EnsureClientDeviceAsync(MySqlConnection conn, string clientDeviceId, MySqlTransaction? transaction = null)
        {
            const string selectSql = @"
                SELECT idThietBi
                FROM thietbi
                WHERE maThietBi = @maThietBi
                LIMIT 1;";

            using (var selectCmd = CreateCommand(selectSql, conn, transaction))
            {
                selectCmd.Parameters.AddWithValue("@maThietBi", clientDeviceId);
                var existing = await selectCmd.ExecuteScalarAsync();
                if (existing != null && existing != DBNull.Value)
                    return Convert.ToInt32(existing);
            }

            var maKichHoat = $"{AppClientActivationPrefix}{clientDeviceId.Replace(AppClientDevicePrefix, string.Empty, StringComparison.OrdinalIgnoreCase)}";
            if (maKichHoat.Length > 100)
                maKichHoat = maKichHoat[..100];

            const string insertSql = @"
                INSERT INTO thietbi (maThietBi, maKichHoat, idTaiKhoan, daKichHoat, thoiGianKichHoat, ngayTao, lanCuoiHoatDong, trangThai, loaiThietBi)
                VALUES (@maThietBi, @maKichHoat, NULL, 1, NOW(), NOW(), NOW(), 'hoat_dong', 'app_client');
                SELECT LAST_INSERT_ID();";

            using var insertCmd = CreateCommand(insertSql, conn, transaction);
            insertCmd.Parameters.AddWithValue("@maThietBi", clientDeviceId);
            insertCmd.Parameters.AddWithValue("@maKichHoat", maKichHoat);
            return Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
        }

        private static async Task ExpireActiveClientSessionsForDeviceAsync(MySqlConnection conn, string clientDeviceId, string? excludeAccessToken = null, MySqlTransaction? transaction = null)
        {
            const string sql = @"
                UPDATE phien_vao_app
                SET trangThai = 'huy'
                WHERE maThietBi = @maThietBi
                  AND trangThai = 'hieu_luc'
                  AND (@excludeAccessToken IS NULL OR accessToken <> @excludeAccessToken);";

            using var cmd = CreateCommand(sql, conn, transaction);
            cmd.Parameters.AddWithValue("@maThietBi", clientDeviceId);
            cmd.Parameters.AddWithValue("@excludeAccessToken", string.IsNullOrWhiteSpace(excludeAccessToken) ? DBNull.Value : excludeAccessToken);
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task TouchDeviceAsync(MySqlConnection conn, int idThietBi, MySqlTransaction? transaction = null)
        {
            const string sql = @"
                UPDATE thietbi
                SET lanCuoiHoatDong = NOW()
                WHERE idThietBi = @idThietBi;";

            using var cmd = CreateCommand(sql, conn, transaction);
            cmd.Parameters.AddWithValue("@idThietBi", idThietBi);
            await cmd.ExecuteNonQueryAsync();
        }

        private static MySqlCommand CreateCommand(string commandText, MySqlConnection conn, MySqlTransaction? transaction = null)
        {
            var cmd = new MySqlCommand(commandText, conn);
            if (transaction is not null)
                cmd.Transaction = transaction;

            return cmd;
        }

        private static bool IsClientManagedDeviceCode(string? maThietBi)
        {
            return !string.IsNullOrWhiteSpace(maThietBi) &&
                   maThietBi.StartsWith(AppClientDevicePrefix, StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeClientDeviceId(string? clientDeviceId)
        {
            return (clientDeviceId ?? string.Empty).Trim().ToUpperInvariant();
        }

        private static int NormalizeDurationDays(object value)
        {
            var days = Convert.ToInt32(value);
            return days > 0 ? days : 1;
        }

        private static string GenerateAccessToken()
        {
            Span<byte> buffer = stackalloc byte[32];
            RandomNumberGenerator.Fill(buffer);
            return Convert.ToHexString(buffer);
        }

        private sealed record PackageInfo(int IdGoi, string TenGoi, int DurationDays, decimal Price);
        private sealed record CassoTransaction(string? TransactionId, string Description, decimal Amount, DateTime? PaidAt);
    }
}
