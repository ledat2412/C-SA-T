using MySqlConnector;

namespace MauiApp1.Services
{
    public class MySqlService
    {
        // Android Emulator thường dùng 10.0.2.2 để trỏ về máy host
        // Nếu chạy Windows trực tiếp thì có thể đổi thành localhost hoặc 127.0.0.1
        private readonly string _connectionString =
            "Server=10.0.2.2;Port=3306;Database=gianhang;User ID=root;Password=;Charset=utf8mb4;";

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
                return conn.State == System.Data.ConnectionState.Open;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string query = "SELECT COUNT(*) FROM taikhoan WHERE username=@u AND password=@p";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);

            var result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return result > 0;
        }
    }
}