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

        public async Task<List<HinhAnhChiNhanh>> GetByChiNhanhAsync(int idGianHang)
        {
            var list = new List<HinhAnhChiNhanh>();

            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();

                const string sql = @"
                    SELECT idHinhAnh, duongDan, idGianHang
                    FROM hinhAnhGianHang
                    WHERE idGianHang = @idGianHang";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@idGianHang", idGianHang);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new HinhAnhChiNhanh
                    {
                        IDHinhAnh = reader.GetInt32("idHinhAnh"),
                        DuongDan = reader.GetString("duongDan"),
                        IDChiNhanh = reader.GetInt32("idGianHang")
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByChiNhanhAsync HinhAnhChiNhanh lỗi: {ex.Message}");
            }

            return list;
        }

        public async Task<string?> GetFirstImagePathByChiNhanhAsync(int idGianHang)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();

                const string sql = @"
                    SELECT duongDan
                    FROM hinhAnhGianHang
                    WHERE idGianHang = @idGianHang
                    ORDER BY idHinhAnh
                    LIMIT 1";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@idGianHang", idGianHang);

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
