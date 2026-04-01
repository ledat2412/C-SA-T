using MySqlConnector;
using System.Data;

namespace MauiApp1.Services
{
    public class MySqlService
    {
        // Android Emulator thường dùng 10.0.2.2 để trỏ về máy host
        // Nếu chạy Windows trực tiếp thì có thể đổi thành localhost hoặc 127.0.0.1
        private readonly string _connectionString =
<<<<<<< HEAD
            "Server=10.0.2.2;Port=3307;Database=gianhang1;User ID=root;Password=bill599199;";
=======
            "Server=10.0.2.2;Port=3306;Database=gianhang;User ID=root;Password=;Charset=utf8mb4;";
>>>>>>> 2e8f476d66d60fa26b4337d437e259a134102afb

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();
<<<<<<< HEAD
                return conn.State == ConnectionState.Open;
=======
                return conn.State == System.Data.ConnectionState.Open;
>>>>>>> 2e8f476d66d60fa26b4337d437e259a134102afb
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LoginAsync(string usernameOrEmail, string password)
        {
            try
            {
                using var conn = GetConnection();
                await conn.OpenAsync();

<<<<<<< HEAD
                const string query = @"
                    SELECT COUNT(*)
                    FROM taiKhoan
                    WHERE (username = @u OR email = @u)
                      AND matKhau = @p
                      AND tinhTrang = 'hoat_dong'";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", usernameOrEmail);
                cmd.Parameters.AddWithValue("@p", password);
=======
            string query = "SELECT COUNT(*) FROM taikhoan WHERE username=@u AND password=@p";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);
>>>>>>> 2e8f476d66d60fa26b4337d437e259a134102afb

                var result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return result > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
