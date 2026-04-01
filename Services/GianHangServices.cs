using MauiApp1.Models;
using MySqlConnector;

namespace MauiApp1.Services
{
    public class GianHangService
    {
        private readonly MySqlService _db;
        private const string DefaultLanguageCode = "vi";

        public GianHangService(MySqlService db)
        {
            _db = db;
        }

        public async Task<List<GianHang>> GetAllAsync(string languageCode = DefaultLanguageCode)
        {
            var list = new List<GianHang>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT 
                    gh.idGianHang,
                    COALESCE(ghnn.ten, gh.ten) AS ten,
                    gh.diaChi,
                    ghnn.moTa,
                    gh.lat,
                    gh.lon,
                    gh.tinhTrang,
                    gh.phiHangThang,
                    gh.ngayDangKy,
                    gh.thoiGianCapNhat
                FROM gianHang gh
                LEFT JOIN ngonNgu nn 
                    ON nn.maNgonNgu = @maNgonNgu
                LEFT JOIN gianHangNgonNgu ghnn 
                    ON ghnn.idGianHang = gh.idGianHang 
                   AND ghnn.idNgonNgu = nn.idNgonNgu
                ORDER BY gh.idGianHang";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@maNgonNgu", languageCode);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(MapGianHang(reader));
            }

            return list;
        }

        public async Task<GianHang?> GetByIdAsync(int idGianHang, string languageCode = DefaultLanguageCode)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();

                const string sql = @"
                    SELECT 
                        gh.idGianHang,
                        COALESCE(ghnn.ten, gh.ten) AS ten,
                        gh.diaChi,
                        ghnn.moTa,
                        gh.lat,
                        gh.lon,
                        gh.tinhTrang,
                        gh.phiHangThang,
                        gh.ngayDangKy,
                        gh.thoiGianCapNhat
                    FROM gianHang gh
                    LEFT JOIN ngonNgu nn 
                        ON nn.maNgonNgu = @maNgonNgu
                    LEFT JOIN gianHangNgonNgu ghnn 
                        ON ghnn.idGianHang = gh.idGianHang 
                       AND ghnn.idNgonNgu = nn.idNgonNgu
                    WHERE gh.idGianHang = @id
                    LIMIT 1";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", idGianHang);
                cmd.Parameters.AddWithValue("@maNgonNgu", languageCode);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return MapGianHang(reader);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByIdAsync GianHang lỗi: {ex.Message}");
            }

            return null;
        }

        private static GianHang MapGianHang(MySqlDataReader reader)
        {
            int ordId = reader.GetOrdinal("idGianHang");
            int ordTen = reader.GetOrdinal("ten");
            int ordDiaChi = reader.GetOrdinal("diaChi");
            int ordMoTa = reader.GetOrdinal("moTa");
            int ordLat = reader.GetOrdinal("lat");
            int ordLon = reader.GetOrdinal("lon");
            int ordTinhTrang = reader.GetOrdinal("tinhTrang");
            int ordPhi = reader.GetOrdinal("phiHangThang");
            int ordNgayDangKy = reader.GetOrdinal("ngayDangKy");
            int ordCapNhat = reader.GetOrdinal("thoiGianCapNhat");

            return new GianHang
            {
                IdGianHang = reader.GetInt32(ordId),
                Ten = reader.IsDBNull(ordTen) ? string.Empty : reader.GetString(ordTen),
                DiaChi = reader.IsDBNull(ordDiaChi) ? string.Empty : reader.GetString(ordDiaChi),
                MoTa = reader.IsDBNull(ordMoTa) ? string.Empty : reader.GetString(ordMoTa),
                Lat = reader.IsDBNull(ordLat) ? null : Convert.ToDouble(reader.GetValue(ordLat)),
                Lon = reader.IsDBNull(ordLon) ? null : Convert.ToDouble(reader.GetValue(ordLon)),
                TinhTrang = reader.IsDBNull(ordTinhTrang) ? null : reader.GetString(ordTinhTrang),
                PhiHangThang = reader.IsDBNull(ordPhi) ? 0 : reader.GetDecimal(ordPhi),
                NgayDangKy = reader.GetDateTime(ordNgayDangKy),
                ThoiGianCapNhat = reader.IsDBNull(ordCapNhat) ? null : reader.GetDateTime(ordCapNhat)
            };
        }
    }
}
