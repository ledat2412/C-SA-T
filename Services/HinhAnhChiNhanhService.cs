using MauiApp1.Models;
using MySqlConnector;

namespace MauiApp1.Services
{
    public class HinhAnhChiNhanhService
    {
        private readonly MySqlService _db;

        public HinhAnhChiNhanhService(MySqlService db)
        {
            _db = db;
        }

        public async Task<List<HinhAnhChiNhanh>> GetByChiNhanhAsync(int idChiNhanh)
        {
            var list = new List<HinhAnhChiNhanh>();

            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();

                string sql = @"
                    SELECT IDHinhAnh, DuongDan, IDChiNhanh
                    FROM hinh_anh_chi_nhanh
                    WHERE IDChiNhanh = @IDChiNhanh";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@IDChiNhanh", idChiNhanh);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new HinhAnhChiNhanh
                    {
                        IDHinhAnh = reader.GetInt32("IDHinhAnh"),
                        DuongDan = reader.GetString("DuongDan"),
                        IDChiNhanh = reader.GetInt32("IDChiNhanh")
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByChiNhanhAsync HinhAnhChiNhanh lỗi: {ex.Message}");
            }

            return list;
        }

        public async Task<string?> GetFirstImagePathByChiNhanhAsync(int idChiNhanh)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();

                string sql = @"
SELECT duongDan
FROM hinhanhgianhang
WHERE idGianHang = @id
LIMIT 1";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@IDChiNhanh", idChiNhanh);

                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetFirstImagePathByChiNhanhAsync lỗi: {ex.Message}");
                return null;
            }
        }
    }
}