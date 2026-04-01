using MySqlConnector;
using System.Data;

namespace MauiApp1.Services
{
    public class MySqlService
    {
        private readonly string _connectionString =
            "Server=10.0.2.2;Port=3307;Database=gianhang1;User ID=root;Password=bill599199;";

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
                return conn.State == ConnectionState.Open;
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

                const string query = @"
                    SELECT COUNT(*)
                    FROM taiKhoan
                    WHERE (username = @u OR email = @u)
                      AND matKhau = @p
                      AND tinhTrang = 'hoat_dong'";

                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", usernameOrEmail);
                cmd.Parameters.AddWithValue("@p", password);

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
