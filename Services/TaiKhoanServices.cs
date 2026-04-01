<<<<<<< HEAD
using MySqlConnector;
=======
﻿using MySqlConnector;
>>>>>>> 2e8f476d66d60fa26b4337d437e259a134102afb

namespace MauiApp1.Services
{
    public class TaiKhoanService
    {
        private readonly MySqlService _db;

        public TaiKhoanService(MySqlService db)
        {
            _db = db;
        }

        public async Task<bool> LoginAsync(string dangNhap, string matKhau)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();

<<<<<<< HEAD
                const string sql = @"
                    SELECT COUNT(*)
                    FROM taiKhoan
=======
                string sql = @"
                    SELECT COUNT(*)
                    FROM taikhoan
>>>>>>> 2e8f476d66d60fa26b4337d437e259a134102afb
                    WHERE (username = @dangNhap OR email = @dangNhap)
                      AND matKhau = @matKhau
                      AND tinhTrang = 'hoat_dong'";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@dangNhap", dangNhap);
                cmd.Parameters.AddWithValue("@matKhau", matKhau);

                var result = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                return result > 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoginAsync lỗi: {ex.Message}");
                return false;
            }
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> 2e8f476d66d60fa26b4337d437e259a134102afb
