using MauiApp1.Models;
using MySqlConnector;

namespace MauiApp1.Services
{
    public class MonAnService
    {
        private readonly MySqlService _db;

        public MonAnService(MySqlService db)
        {
            _db = db;
        }

        public async Task<List<MonAn>> GetAllAsync()
        {
            var list = new List<MonAn>();

            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();

                string sql = @"
                    SELECT IDMon, TenMon, ThongTinMon, DonGia, TrangThai, IDChiNhanh
                    FROM mon_an";

                using var cmd = new MySqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new MonAn
                    {
                        IDMon = reader.GetInt32("IDMon"),
                        TenMon = reader.GetString("TenMon"),
                        ThongTinMon = reader.IsDBNull(reader.GetOrdinal("ThongTinMon"))
                            ? null : reader.GetString("ThongTinMon"),
                        DonGia = reader.GetDecimal("DonGia"),
                        TrangThai = reader.IsDBNull(reader.GetOrdinal("TrangThai"))
                            ? null : reader.GetString("TrangThai"),
                        IDChiNhanh = reader.GetInt32("IDChiNhanh")
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllAsync MonAn lỗi: {ex.Message}");
            }

            return list;
        }

        public async Task<List<MonAn>> GetByChiNhanhAsync(int idChiNhanh)
        {
            var list = new List<MonAn>();

            try
            {
                using var conn = _db.GetConnection();
                await conn.OpenAsync();

                string sql = @"
                    SELECT IDMon, TenMon, ThongTinMon, DonGia, TrangThai, IDChiNhanh
                    FROM mon_an
                    WHERE IDChiNhanh = @IDChiNhanh";

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@IDChiNhanh", idChiNhanh);

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new MonAn
                    {
                        IDMon = reader.GetInt32("IDMon"),
                        TenMon = reader.GetString("TenMon"),
                        ThongTinMon = reader.IsDBNull(reader.GetOrdinal("ThongTinMon"))
                            ? null : reader.GetString("ThongTinMon"),
                        DonGia = reader.GetDecimal("DonGia"),
                        TrangThai = reader.IsDBNull(reader.GetOrdinal("TrangThai"))
                            ? null : reader.GetString("TrangThai"),
                        IDChiNhanh = reader.GetInt32("IDChiNhanh")
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByChiNhanhAsync MonAn lỗi: {ex.Message}");
            }

            return list;
        }
    }
}