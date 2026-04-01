using MauiApp1.Models;
using MySqlConnector;

namespace MauiApp1.Services
{
    public class MonAnService
    {
        private readonly MySqlService _db;
        private const string DefaultLanguageCode = "vi";

        public MonAnService(MySqlService db)
        {
            _db = db;
        }

        public async Task<List<MonAn>> GetAllAsync(string languageCode = DefaultLanguageCode)
        {
            var list = new List<MonAn>();

            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();

                const string sql = @"
                    SELECT 
                        ma.idMonAn,
                        COALESCE(mann.ten, ma.ten) AS ten,
                        mann.moTa,
                        ma.donGia,
                        ma.tinhTrang,
                        ma.idGianHang
                    FROM monAn ma
                    LEFT JOIN ngonNgu nn
                        ON nn.maNgonNgu = @maNgonNgu
                    LEFT JOIN monAnNgonNgu mann
                        ON mann.idMonAn = ma.idMonAn
                       AND mann.idNgonNgu = nn.idNgonNgu
                    ORDER BY ma.idMonAn";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@maNgonNgu", languageCode);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(MapMonAn(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllAsync MonAn lỗi: {ex.Message}");
            }

            return list;
        }

        public async Task<List<MonAn>> GetByChiNhanhAsync(int idGianHang, string languageCode = DefaultLanguageCode)
        {
            var list = new List<MonAn>();

            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();

                const string sql = @"
                    SELECT 
                        ma.idMonAn,
                        COALESCE(mann.ten, ma.ten) AS ten,
                        mann.moTa,
                        ma.donGia,
                        ma.tinhTrang,
                        ma.idGianHang
                    FROM monAn ma
                    LEFT JOIN ngonNgu nn
                        ON nn.maNgonNgu = @maNgonNgu
                    LEFT JOIN monAnNgonNgu mann
                        ON mann.idMonAn = ma.idMonAn
                       AND mann.idNgonNgu = nn.idNgonNgu
                    WHERE ma.idGianHang = @idGianHang
                    ORDER BY ma.idMonAn";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@idGianHang", idGianHang);
                cmd.Parameters.AddWithValue("@maNgonNgu", languageCode);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(MapMonAn(reader));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByChiNhanhAsync MonAn lỗi: {ex.Message}");
            }

            return list;
        }

        private static MonAn MapMonAn(MySqlDataReader reader)
        {
            int ordId = reader.GetOrdinal("idMonAn");
            int ordTen = reader.GetOrdinal("ten");
            int ordMoTa = reader.GetOrdinal("moTa");
            int ordDonGia = reader.GetOrdinal("donGia");
            int ordTinhTrang = reader.GetOrdinal("tinhTrang");
            int ordGianHang = reader.GetOrdinal("idGianHang");

            return new MonAn
            {
                IDMon = reader.GetInt32(ordId),
                TenMon = reader.IsDBNull(ordTen) ? string.Empty : reader.GetString(ordTen),
                ThongTinMon = reader.IsDBNull(ordMoTa) ? string.Empty : reader.GetString(ordMoTa),
                DonGia = reader.GetDecimal(ordDonGia),
                TrangThai = reader.IsDBNull(ordTinhTrang) ? string.Empty : reader.GetString(ordTinhTrang),
                IDChiNhanh = reader.GetInt32(ordGianHang)
            };
        }
    }
}
