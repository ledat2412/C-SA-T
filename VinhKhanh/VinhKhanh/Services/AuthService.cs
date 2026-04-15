using MySqlConnector;
using VinhKhanh.Data;
using VinhKhanh.Dtos;

namespace VinhKhanh.Services
{
    public class AuthService
    {
        private readonly MySqlDbContext _db;

        public AuthService(MySqlDbContext db)
        {
            _db = db;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var account = !string.IsNullOrWhiteSpace(request.Username)
                ? request.Username.Trim()
                : request.Email.Trim();

            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(request.MatKhau))
            {
                return new LoginResponseDto
                {
                    Success = false,
                    Message = "Sai tài khoản hoặc mật khẩu."
                };
            }

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT
                    tk.idTaiKhoan,
                    tk.username,
                    tk.email,
                    tk.loaiTaiKhoan,
                    ad.idAdmin,
                    cql.idChuQuanLy,
                    COALESCE(ad.hoTen, cql.hoTen) AS hoTen
                FROM taikhoan tk
                LEFT JOIN admin ad ON ad.idTaiKhoan = tk.idTaiKhoan
                LEFT JOIN chu_quan_ly cql ON cql.idTaiKhoan = tk.idTaiKhoan
                WHERE (tk.username = @account OR tk.email = @account)
                  AND tk.matKhau = @matKhau
                  AND tk.tinhTrang = 'hoat_dong'
                LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@account", account);
            cmd.Parameters.AddWithValue("@matKhau", request.MatKhau);

            using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new LoginResponseDto
                {
                    Success = true,
                    Message = "Đăng nhập thành công.",
                    IdTaiKhoan = reader.GetInt32("idTaiKhoan"),
                    Username = reader["username"]?.ToString(),
                    Email = reader["email"]?.ToString(),
                    LoaiTaiKhoan = reader["loaiTaiKhoan"]?.ToString(),
                    IdAdmin = reader["idAdmin"] == DBNull.Value ? null : Convert.ToInt32(reader["idAdmin"]),
                    IdChuQuanLy = reader["idChuQuanLy"] == DBNull.Value ? null : Convert.ToInt32(reader["idChuQuanLy"]),
                    HoTen = reader["hoTen"]?.ToString()
                };
            }

            return new LoginResponseDto
            {
                Success = false,
                Message = "Sai tài khoản hoặc mật khẩu."
            };
        }
    }
}
