using System.Security.Cryptography;
using MySqlConnector;
using VinhKhanh.Data;
using VinhKhanh.Dtos;

namespace VinhKhanh.Services
{
    public class AccessSessionService
    {
        private readonly MySqlDbContext _db;
        private readonly IConfiguration _config;

        public AccessSessionService(MySqlDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
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

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string selectDeviceSql = @"
                SELECT maThietBi, daKichHoat, trangThai
                FROM thietbi
                WHERE maThietBi = @maThietBi
                LIMIT 1;";

            using var selectDeviceCmd = new MySqlCommand(selectDeviceSql, conn);
            selectDeviceCmd.Parameters.AddWithValue("@maThietBi", request.MaThietBi);

            using var reader = await selectDeviceCmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
            {
                return new AccessSessionResponseDto
                {
                    Success = false,
                    Message = "Không tìm thấy thiết bị."
                };
            }

            var daKichHoat = Convert.ToBoolean(reader["daKichHoat"]);
            var trangThaiThietBi = reader["trangThai"]?.ToString();
            await reader.CloseAsync();

            if (!daKichHoat || !string.Equals(trangThaiThietBi, "hoat_dong", StringComparison.OrdinalIgnoreCase))
            {
                return new AccessSessionResponseDto
                {
                    Success = false,
                    Message = "Thiết bị chưa được kích hoạt hoặc đang không hoạt động.",
                    MaThietBi = request.MaThietBi
                };
            }

            const string expireOldSessionsSql = @"
                UPDATE phien_vao_app
                SET trangThai = 'het_han'
                WHERE maThietBi = @maThietBi
                  AND trangThai = 'hieu_luc';";

            using (var expireCmd = new MySqlCommand(expireOldSessionsSql, conn))
            {
                expireCmd.Parameters.AddWithValue("@maThietBi", request.MaThietBi);
                await expireCmd.ExecuteNonQueryAsync();
            }

            var batDauLuc = DateTime.UtcNow;
            var hetHanLuc = batDauLuc.AddMinutes(GetSessionMinutes());
            var accessToken = GenerateAccessToken();

            const string insertSql = @"
                INSERT INTO phien_vao_app (maThietBi, qrRaw, accessToken, batDauLuc, hetHanLuc, trangThai)
                VALUES (@maThietBi, @qrRaw, @accessToken, @batDauLuc, @hetHanLuc, 'hieu_luc');";

            using (var insertCmd = new MySqlCommand(insertSql, conn))
            {
                insertCmd.Parameters.AddWithValue("@maThietBi", request.MaThietBi);
                insertCmd.Parameters.AddWithValue("@qrRaw", string.IsNullOrWhiteSpace(request.QrRaw) ? DBNull.Value : request.QrRaw);
                insertCmd.Parameters.AddWithValue("@accessToken", accessToken);
                insertCmd.Parameters.AddWithValue("@batDauLuc", batDauLuc);
                insertCmd.Parameters.AddWithValue("@hetHanLuc", hetHanLuc);
                await insertCmd.ExecuteNonQueryAsync();
            }

            const string updateDeviceSql = @"
                UPDATE thietbi
                SET lanCuoiHoatDong = NOW()
                WHERE maThietBi = @maThietBi;";

            using (var updateDeviceCmd = new MySqlCommand(updateDeviceSql, conn))
            {
                updateDeviceCmd.Parameters.AddWithValue("@maThietBi", request.MaThietBi);
                await updateDeviceCmd.ExecuteNonQueryAsync();
            }

            return new AccessSessionResponseDto
            {
                Success = true,
                Message = "Tạo phiên vào app thành công.",
                MaThietBi = request.MaThietBi,
                AccessToken = accessToken,
                BatDauLuc = batDauLuc,
                HetHanLuc = hetHanLuc,
                TrangThai = "hieu_luc"
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

        private int GetSessionMinutes()
        {
            var sessionMinutes = _config.GetValue<int?>("AccessControl:SessionMinutes");
            var value = sessionMinutes.GetValueOrDefault(30);
            return value > 0 ? value : 30;
        }

        private static string GenerateAccessToken()
        {
            Span<byte> buffer = stackalloc byte[32];
            RandomNumberGenerator.Fill(buffer);
            return Convert.ToHexString(buffer);
        }
    }
}
