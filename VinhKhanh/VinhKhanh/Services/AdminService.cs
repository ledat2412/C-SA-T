using MySqlConnector;
using VinhKhanh.Data;
using VinhKhanh.Dtos;

namespace VinhKhanh.Services
{
    public class AdminService
    {
        private readonly MySqlDbContext _db;

        public AdminService(MySqlDbContext db)
        {
            _db = db;
        }

        public async Task<AdminSummaryDto> GetSummaryAsync()
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            return new AdminSummaryDto
            {
                TongGianHang = await ExecuteCountAsync(conn, "SELECT COUNT(*) FROM gianhang;"),
                TongChuQuanLy = await ExecuteCountAsync(conn, "SELECT COUNT(*) FROM chu_quan_ly;"),
                TongThietBi = await ExecuteCountAsync(conn, "SELECT COUNT(*) FROM thietbi;"),
                ThietBiDangHoatDong = await ExecuteCountAsync(conn, "SELECT COUNT(*) FROM thietbi WHERE trangThai = 'hoat_dong';")
            };
        }

        public async Task<List<AdminStoreDto>> GetStoresAsync()
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT
                    gh.idGianHang,
                    gh.ten,
                    gh.diaChi,
                    gh.tinhTrang,
                    cql.idChuQuanLy,
                    cql.hoTen AS tenChuQuanLy,
                    tk.email AS emailChuQuanLy,
                    tk.username AS usernameChuQuanLy
                FROM gianhang gh
                LEFT JOIN chu_quan_ly cql ON cql.idChuQuanLy = gh.idChuQuanLy
                LEFT JOIN taikhoan tk ON tk.idTaiKhoan = cql.idTaiKhoan
                ORDER BY gh.idGianHang;";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            var list = new List<AdminStoreDto>();
            while (await reader.ReadAsync())
            {
                list.Add(new AdminStoreDto
                {
                    IdGianHang = reader.GetInt32("idGianHang"),
                    Ten = reader["ten"]?.ToString() ?? string.Empty,
                    DiaChi = reader["diaChi"]?.ToString(),
                    TinhTrang = reader["tinhTrang"]?.ToString(),
                    IdChuQuanLy = reader["idChuQuanLy"] == DBNull.Value ? null : Convert.ToInt32(reader["idChuQuanLy"]),
                    TenChuQuanLy = reader["tenChuQuanLy"]?.ToString(),
                    EmailChuQuanLy = reader["emailChuQuanLy"]?.ToString(),
                    UsernameChuQuanLy = reader["usernameChuQuanLy"]?.ToString()
                });
            }

            return list;
        }

        private static async Task<int> ExecuteCountAsync(MySqlConnection conn, string sql)
        {
            using var cmd = new MySqlCommand(sql, conn);
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
    }
}
