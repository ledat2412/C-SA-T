using System.Security.Cryptography;
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

        public AccessSessionService(MySqlDbContext db, PackageAccessEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
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
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.'))
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
                invoiceCmd.Parameters.AddWithValue("@email", email);
                invoiceCmd.Parameters.AddWithValue("@tongTien", package.Price);
                invoiceCmd.Parameters.AddWithValue("@ghiChu", "Bypass thanh toan QR de test package access.");
                invoiceId = Convert.ToInt32(await invoiceCmd.ExecuteScalarAsync());
            }

            await TouchDeviceAsync(conn, deviceId);

            var emailResult = request.SendEmail
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

        private static async Task<PackageInfo?> ResolvePackageAsync(MySqlConnection conn, string maThietBi, int? requestedPackageId)
        {
            const string explicitPackageSql = @"
                SELECT idGoi, ten, thoiHanNgay, gia
                FROM goidichvu
                WHERE idGoi = @idGoi
                  AND trangThai = 'hoat_dong'
                LIMIT 1;";

            if (requestedPackageId.HasValue)
            {
                using var explicitCmd = new MySqlCommand(explicitPackageSql, conn);
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

            using var lastRegisteredCmd = new MySqlCommand(lastRegisteredPackageSql, conn);
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

        private static async Task<int> EnsureClientDeviceAsync(MySqlConnection conn, string clientDeviceId)
        {
            const string selectSql = @"
                SELECT idThietBi
                FROM thietbi
                WHERE maThietBi = @maThietBi
                LIMIT 1;";

            using (var selectCmd = new MySqlCommand(selectSql, conn))
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

            using var insertCmd = new MySqlCommand(insertSql, conn);
            insertCmd.Parameters.AddWithValue("@maThietBi", clientDeviceId);
            insertCmd.Parameters.AddWithValue("@maKichHoat", maKichHoat);
            return Convert.ToInt32(await insertCmd.ExecuteScalarAsync());
        }

        private static async Task ExpireActiveClientSessionsForDeviceAsync(MySqlConnection conn, string clientDeviceId, string? excludeAccessToken = null)
        {
            const string sql = @"
                UPDATE phien_vao_app
                SET trangThai = 'huy'
                WHERE maThietBi = @maThietBi
                  AND trangThai = 'hieu_luc'
                  AND (@excludeAccessToken IS NULL OR accessToken <> @excludeAccessToken);";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@maThietBi", clientDeviceId);
            cmd.Parameters.AddWithValue("@excludeAccessToken", string.IsNullOrWhiteSpace(excludeAccessToken) ? DBNull.Value : excludeAccessToken);
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task TouchDeviceAsync(MySqlConnection conn, int idThietBi)
        {
            const string sql = @"
                UPDATE thietbi
                SET lanCuoiHoatDong = NOW()
                WHERE idThietBi = @idThietBi;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idThietBi", idThietBi);
            await cmd.ExecuteNonQueryAsync();
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
    }
}
