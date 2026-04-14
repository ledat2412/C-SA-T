using System.Security.Cryptography;
using MySqlConnector;
using VinhKhanh.Data;
using VinhKhanh.Dtos;

namespace VinhKhanh.Services
{
    public class AccessSessionService
    {
        private readonly MySqlDbContext _db;

        public AccessSessionService(MySqlDbContext db)
        {
            _db = db;
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

        public async Task<ValidateAccessResponseDto> ValidateAsync(string accessToken)
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
                SELECT maThietBi, batDauLuc, hetHanLuc, trangThai
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
                    WHERE accessToken = @accessToken
                      AND trangThai = 'hieu_luc';";

                using var expireCmd = new MySqlCommand(expireSql, conn);
                expireCmd.Parameters.AddWithValue("@accessToken", accessToken);
                await expireCmd.ExecuteNonQueryAsync();
                trangThai = "het_han";
            }

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

        private static async Task<PackageInfo?> ResolvePackageAsync(MySqlConnection conn, string maThietBi, int? requestedPackageId)
        {
            const string explicitPackageSql = @"
                SELECT idGoi, ten, thoiHanNgay
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
                        NormalizeDurationDays(explicitReader["thoiHanNgay"]));
                }

                return null;
            }

            const string lastRegisteredPackageSql = @"
                SELECT gdv.idGoi, gdv.ten, gdv.thoiHanNgay
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
                NormalizeDurationDays(lastRegisteredReader["thoiHanNgay"]));
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

        private sealed record PackageInfo(int IdGoi, string TenGoi, int DurationDays);
    }
}
