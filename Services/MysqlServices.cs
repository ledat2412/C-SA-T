using MySqlConnector;

namespace MauiApp1.Services
{
    public class MySqlService
    {
        private readonly string _connectionString =
            "Server=10.0.2.2;Port=3306;Database=gianhang;User ID=root;Password=;";

        public async Task<bool> TestConnectionAsync()
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            return true;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
                SELECT COUNT(*)
                FROM TaiKhoan
                WHERE email = @username
                  AND mat_khau = @password";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@password", password);

            var result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return result > 0;
        }
    }
}