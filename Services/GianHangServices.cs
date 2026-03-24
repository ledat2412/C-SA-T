using MauiApp1.Models;
using MySqlConnector;
using System.Data;

namespace MauiApp1.Services
{
    public class GianHangService
    {
        private readonly MySqlService _db;

        public GianHangService(MySqlService db)
        {
            _db = db;
        }

        public async Task<List<GianHang>> GetAllAsync()
        {
            var list = new List<GianHang>();

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            string sql = @"
        SELECT idGianHang, ten, diaChi, moTa, lat, lon, tinhTrang, phiHangThang, ngayDangKy, thoiGianCapNhat
        FROM gianhang";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new GianHang
                {
                    IdGianHang = reader.GetInt32("idGianHang"),
                    Ten = reader.GetString("ten"),
                    DiaChi = reader.IsDBNull("diaChi") ? "" : reader.GetString("diaChi"),
                    MoTa = reader.IsDBNull("moTa") ? "" : reader.GetString("moTa"),
                    Lat = reader.IsDBNull("lat") ? null : reader.GetDouble("lat"),
                    Lon = reader.IsDBNull("lon") ? null : reader.GetDouble("lon"),
                    TinhTrang = reader.IsDBNull("tinhTrang") ? null : reader.GetString("tinhTrang"),
                    PhiHangThang = reader.GetDecimal("phiHangThang"),
                    NgayDangKy = reader.GetDateTime("ngayDangKy"),
                    ThoiGianCapNhat = reader.IsDBNull("thoiGianCapNhat")
                        ? null : reader.GetDateTime("thoiGianCapNhat")
                });
            }

            return list;
        }

        public async Task<GianHang?> GetByIdAsync(int idChiNhanh)
        {
            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();

                string sql = @"
                    SELECT IDChiNhanh, ten, DiaChi, Lat, Lon, PhiHangThang, 
                           TinhTrangChiNhanh, MoTaChiNhanh, NgayDangKy
                    FROM gian_hang
                    WHERE IDChiNhanh = @ID
                    LIMIT 1";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@ID", idChiNhanh);

                using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new GianHang
                    {
                        IdGianHang = reader.GetInt32("idGianHang"),
                        Ten = reader.GetString("ten"),
                        DiaChi = reader.IsDBNull(reader.GetOrdinal("diaChi"))
                            ? null : reader.GetString("diaChi"),
                        MoTa = reader.IsDBNull(reader.GetOrdinal("moTa"))
                            ? null : reader.GetString("moTa"),
                        Lat = reader.IsDBNull(reader.GetOrdinal("lat"))
                            ? null : Convert.ToDouble(reader["lat"]),
                        Lon = reader.IsDBNull(reader.GetOrdinal("lon"))
                            ? null : Convert.ToDouble(reader["lon"]),
                        TinhTrang = reader.GetString("tinhTrang"),
                        PhiHangThang = reader.GetDecimal("phiHangThang"),
                        NgayDangKy = reader.GetDateTime("ngayDangKy"),
                        ThoiGianCapNhat = reader.IsDBNull(reader.GetOrdinal("thoiGianCapNhat"))
                            ? null : reader.GetDateTime("thoiGianCapNhat")
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByIdAsync GianHang lỗi: {ex.Message}");
            }

            return null;
        }
    }
}