using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MySqlConnector;
using VinhKhanh.Data;
using VinhKhanh.Dtos;

namespace VinhKhanh.Services
{
    public class StoreManagementService
    {
        private static readonly HashSet<string> StoreStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "dang_hoat_dong",
            "tam_ngung",
            "dong_cua"
        };

        private static readonly HashSet<string> FoodStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "con_ban",
            "het_mon",
            "ngung_ban"
        };

        private readonly MySqlDbContext _db;

        public StoreManagementService(MySqlDbContext db)
        {
            _db = db;
        }

        public async Task<OwnerStoreDto> CreateStoreAsync(UpsertStoreRequestDto request, int ownerId)
        {
            ValidateStoreRequest(request);

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                INSERT INTO gianhang (idChuQuanLy, ten, diaChi, lat, lon, tinhTrang, phiHangThang, ngayDangKy, thoiGianCapNhat)
                VALUES (@idChuQuanLy, @ten, @diaChi, @lat, @lon, @tinhTrang, @phiHangThang, NOW(), NOW());
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idChuQuanLy", ownerId);
            cmd.Parameters.AddWithValue("@ten", request.Ten);
            cmd.Parameters.AddWithValue("@diaChi", (object?)request.DiaChi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lat", request.Lat.HasValue ? request.Lat.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@lon", request.Lon.HasValue ? request.Lon.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@tinhTrang", NormalizeStoreStatus(request.TinhTrang));
            cmd.Parameters.AddWithValue("@phiHangThang", request.PhiHangThang);

            var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return await GetOwnerStoreByIdAsync(newId, conn);
        }

        public async Task<OwnerStoreDto?> UpdateStoreAsync(int idGianHang, UpsertStoreRequestDto request)
        {
            ValidateStoreRequest(request);

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                UPDATE gianhang
                SET idChuQuanLy = COALESCE(@idChuQuanLy, idChuQuanLy),
                    ten = @ten,
                    diaChi = @diaChi,
                    lat = @lat,
                    lon = @lon,
                    tinhTrang = @tinhTrang,
                    phiHangThang = @phiHangThang,
                    thoiGianCapNhat = NOW()
                WHERE idGianHang = @idGianHang;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idGianHang", idGianHang);
            cmd.Parameters.AddWithValue("@idChuQuanLy", request.IdChuQuanLy.HasValue ? request.IdChuQuanLy.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@ten", request.Ten);
            cmd.Parameters.AddWithValue("@diaChi", (object?)request.DiaChi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lat", request.Lat.HasValue ? request.Lat.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@lon", request.Lon.HasValue ? request.Lon.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@tinhTrang", NormalizeStoreStatus(request.TinhTrang));
            cmd.Parameters.AddWithValue("@phiHangThang", request.PhiHangThang);

            var rows = await cmd.ExecuteNonQueryAsync();
            if (rows <= 0)
                return null;

            return await GetOwnerStoreByIdAsync(idGianHang, conn);
        }

        public async Task<OperationResultDto> UpdateStoreStatusAsync(int idGianHang, string tinhTrang)
        {
            var status = NormalizeStoreStatus(tinhTrang);

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                UPDATE gianhang
                SET tinhTrang = @tinhTrang,
                    thoiGianCapNhat = NOW()
                WHERE idGianHang = @idGianHang;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idGianHang", idGianHang);
            cmd.Parameters.AddWithValue("@tinhTrang", status);

            var rows = await cmd.ExecuteNonQueryAsync();
            return new OperationResultDto
            {
                Success = rows > 0,
                Message = rows > 0 ? "Cap nhat tinh trang gian hang thanh cong." : "Khong tim thay gian hang."
            };
        }

        public async Task<MonAnDto> CreateFoodAsync(UpsertFoodRequestDto request)
        {
            ValidateFoodRequest(request);

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                INSERT INTO monan (idGianHang, ten, donGia, thoiGianCapNhat, tinhTrang)
                VALUES (@idGianHang, @ten, @donGia, NOW(), @tinhTrang);
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idGianHang", request.IdGianHang);
            cmd.Parameters.AddWithValue("@ten", request.Ten);
            cmd.Parameters.AddWithValue("@donGia", request.DonGia);
            cmd.Parameters.AddWithValue("@tinhTrang", NormalizeFoodStatus(request.TinhTrang));

            var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return await GetFoodByIdAsync(newId, conn);
        }

        public async Task<MonAnDto?> UpdateFoodAsync(int idMonAn, UpsertFoodRequestDto request)
        {
            ValidateFoodRequest(request);

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                UPDATE monan
                SET idGianHang = @idGianHang,
                    ten = @ten,
                    donGia = @donGia,
                    thoiGianCapNhat = NOW(),
                    tinhTrang = @tinhTrang
                WHERE idMonAn = @idMonAn;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idMonAn", idMonAn);
            cmd.Parameters.AddWithValue("@idGianHang", request.IdGianHang);
            cmd.Parameters.AddWithValue("@ten", request.Ten);
            cmd.Parameters.AddWithValue("@donGia", request.DonGia);
            cmd.Parameters.AddWithValue("@tinhTrang", NormalizeFoodStatus(request.TinhTrang));

            var rows = await cmd.ExecuteNonQueryAsync();
            if (rows <= 0)
                return null;

            return await GetFoodByIdAsync(idMonAn, conn);
        }

        public async Task<OperationResultDto> UpdateFoodStatusAsync(int idMonAn, string tinhTrang)
        {
            var status = NormalizeFoodStatus(tinhTrang);

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                UPDATE monan
                SET tinhTrang = @tinhTrang,
                    thoiGianCapNhat = NOW()
                WHERE idMonAn = @idMonAn;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idMonAn", idMonAn);
            cmd.Parameters.AddWithValue("@tinhTrang", status);

            var rows = await cmd.ExecuteNonQueryAsync();
            return new OperationResultDto
            {
                Success = rows > 0,
                Message = rows > 0 ? "Cap nhat tinh trang mon an thanh cong." : "Khong tim thay mon an."
            };
        }

        public async Task<List<MonAnDto>> GetFoodsByStoreAsync(int idGianHang)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT idMonAn, idGianHang, ten, donGia, tinhTrang
                FROM monan
                WHERE idGianHang = @idGianHang
                ORDER BY idMonAn;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idGianHang", idGianHang);

            using var reader = await cmd.ExecuteReaderAsync();
            var list = new List<MonAnDto>();
            while (await reader.ReadAsync())
            {
                list.Add(new MonAnDto
                {
                    IdMonAn = reader.GetInt32("idMonAn"),
                    IdGianHang = reader.GetInt32("idGianHang"),
                    Ten = reader["ten"]?.ToString() ?? string.Empty,
                    DonGia = reader.GetDecimal("donGia"),
                    TinhTrang = reader["tinhTrang"]?.ToString()
                });
            }

            return list;
        }

        public async Task<StoreDetailDto?> GetStoreByIdAsync(int idGianHang, string lang = "vi")
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string sql = @"
                SELECT
                    gh.idGianHang,
                    gh.ten,
                    gh.diaChi,
                    gh.lat,
                    gh.lon,
                    gh.tinhTrang,
                    gh.phiHangThang,
                    gh.ngayDangKy,
                    gh.thoiGianCapNhat,
                    ghnn.moTa,
                    cql.idChuQuanLy,
                    cql.hoTen AS tenChuQuanLy,
                    tk.email AS emailChuQuanLy,
                    tk.username AS usernameChuQuanLy,
                    (
                        SELECT hgg.duongDan
                        FROM hinhanhgianhang hgg
                        WHERE hgg.idGianHang = gh.idGianHang
                        ORDER BY hgg.idHinhAnh
                        LIMIT 1
                    ) AS hinhAnh
                FROM gianhang gh
                LEFT JOIN chu_quan_ly cql ON cql.idChuQuanLy = gh.idChuQuanLy
                LEFT JOIN taikhoan tk ON tk.idTaiKhoan = cql.idTaiKhoan
                LEFT JOIN ngonngu nn ON nn.maNgonNgu = @lang
                LEFT JOIN gianhangngonngu ghnn
                    ON ghnn.idGianHang = gh.idGianHang
                    AND ghnn.idNgonNgu = nn.idNgonNgu
                WHERE gh.idGianHang = @idGianHang
                LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idGianHang", idGianHang);
            cmd.Parameters.AddWithValue("@lang", lang);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new StoreDetailDto
            {
                IdGianHang = reader.GetInt32("idGianHang"),
                Ten = reader["ten"]?.ToString() ?? string.Empty,
                DiaChi = reader["diaChi"]?.ToString(),
                MoTa = reader["moTa"]?.ToString(),
                HinhAnh = NormalizeImagePathForWeb(reader["hinhAnh"]?.ToString()),
                Lat = reader["lat"] == DBNull.Value ? null : Convert.ToDouble(reader["lat"]),
                Lon = reader["lon"] == DBNull.Value ? null : Convert.ToDouble(reader["lon"]),
                TinhTrang = reader["tinhTrang"]?.ToString(),
                PhiHangThang = reader.GetDecimal("phiHangThang"),
                NgayDangKy = Convert.ToDateTime(reader["ngayDangKy"]),
                ThoiGianCapNhat = reader["thoiGianCapNhat"] == DBNull.Value ? null : Convert.ToDateTime(reader["thoiGianCapNhat"]),
                IdChuQuanLy = reader["idChuQuanLy"] == DBNull.Value ? null : Convert.ToInt32(reader["idChuQuanLy"]),
                TenChuQuanLy = reader["tenChuQuanLy"]?.ToString(),
                EmailChuQuanLy = reader["emailChuQuanLy"]?.ToString(),
                UsernameChuQuanLy = reader["usernameChuQuanLy"]?.ToString()
            };
        }

        public async Task<string?> SaveStoreImageAsync(int idGianHang, IFormFile image, IWebHostEnvironment env)
        {
            if (image == null || image.Length <= 0)
                throw new ArgumentException("File anh khong hop le.");

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string existsSql = @"
                SELECT COUNT(*)
                FROM gianhang
                WHERE idGianHang = @idGianHang;";

            using (var existsCmd = new MySqlCommand(existsSql, conn))
            {
                existsCmd.Parameters.AddWithValue("@idGianHang", idGianHang);
                var exists = Convert.ToInt32(await existsCmd.ExecuteScalarAsync());
                if (exists <= 0)
                    return null;
            }

            int? existingImageId = null;
            string? existingImagePath = null;

            const string currentImageSql = @"
                SELECT idHinhAnh, duongDan
                FROM hinhanhgianhang
                WHERE idGianHang = @idGianHang
                ORDER BY idHinhAnh
                LIMIT 1;";

            using (var currentImageCmd = new MySqlCommand(currentImageSql, conn))
            {
                currentImageCmd.Parameters.AddWithValue("@idGianHang", idGianHang);
                using var reader = await currentImageCmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    existingImageId = reader.GetInt32("idHinhAnh");
                    existingImagePath = reader["duongDan"]?.ToString();
                }
            }

            var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
            var targetFolder = Path.Combine(webRoot, "images", "stores");
            Directory.CreateDirectory(targetFolder);

            var extension = Path.GetExtension(image.FileName);
            if (string.IsNullOrWhiteSpace(extension) || extension.Length > 10)
                extension = ".jpg";

            var fileName = $"store_{idGianHang}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension.ToLowerInvariant()}";
            var fullPath = Path.Combine(targetFolder, fileName);
            var dbPath = $"images/stores/{fileName}";

            using (var stream = File.Create(fullPath))
            {
                await image.CopyToAsync(stream);
            }

            if (existingImageId.HasValue)
            {
                const string updateImageSql = @"
                    UPDATE hinhanhgianhang
                    SET duongDan = @duongDan
                    WHERE idHinhAnh = @idHinhAnh;";

                using var updateImageCmd = new MySqlCommand(updateImageSql, conn);
                updateImageCmd.Parameters.AddWithValue("@duongDan", dbPath);
                updateImageCmd.Parameters.AddWithValue("@idHinhAnh", existingImageId.Value);
                await updateImageCmd.ExecuteNonQueryAsync();
            }
            else
            {
                const string insertImageSql = @"
                    INSERT INTO hinhanhgianhang (idGianHang, duongDan)
                    VALUES (@idGianHang, @duongDan);";

                using var insertImageCmd = new MySqlCommand(insertImageSql, conn);
                insertImageCmd.Parameters.AddWithValue("@idGianHang", idGianHang);
                insertImageCmd.Parameters.AddWithValue("@duongDan", dbPath);
                await insertImageCmd.ExecuteNonQueryAsync();
            }

            DeleteManagedImageIfNeeded(existingImagePath, webRoot, dbPath);

            return NormalizeImagePathForWeb(dbPath);
        }

        private async Task<OwnerStoreDto> GetOwnerStoreByIdAsync(int idGianHang, MySqlConnection conn)
        {
            const string sql = @"
                SELECT
                    gh.idGianHang,
                    gh.ten,
                    gh.diaChi,
                    gh.lat,
                    gh.lon,
                    gh.tinhTrang,
                    (
                        SELECT hgg.duongDan
                        FROM hinhanhgianhang hgg
                        WHERE hgg.idGianHang = gh.idGianHang
                        ORDER BY hgg.idHinhAnh
                        LIMIT 1
                    ) AS hinhAnh,
                    gh.phiHangThang,
                    gh.ngayDangKy,
                    gh.thoiGianCapNhat
                FROM gianhang gh
                WHERE gh.idGianHang = @idGianHang
                LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idGianHang", idGianHang);

            using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            return new OwnerStoreDto
            {
                IdGianHang = reader.GetInt32("idGianHang"),
                Ten = reader["ten"]?.ToString() ?? string.Empty,
                DiaChi = reader["diaChi"]?.ToString(),
                Lat = reader["lat"] == DBNull.Value ? null : Convert.ToDouble(reader["lat"]),
                Lon = reader["lon"] == DBNull.Value ? null : Convert.ToDouble(reader["lon"]),
                TinhTrang = reader["tinhTrang"]?.ToString(),
                HinhAnh = NormalizeImagePathForWeb(reader["hinhAnh"]?.ToString()),
                PhiHangThang = reader.GetDecimal("phiHangThang"),
                NgayDangKy = Convert.ToDateTime(reader["ngayDangKy"]),
                ThoiGianCapNhat = reader["thoiGianCapNhat"] == DBNull.Value ? null : Convert.ToDateTime(reader["thoiGianCapNhat"])
            };
        }

        private async Task<MonAnDto> GetFoodByIdAsync(int idMonAn, MySqlConnection conn)
        {
            const string sql = @"
                SELECT idMonAn, idGianHang, ten, donGia, tinhTrang, thoiGianCapNhat
                FROM monan
                WHERE idMonAn = @idMonAn
                LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idMonAn", idMonAn);

            using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            return new MonAnDto
            {
                IdMonAn = reader.GetInt32("idMonAn"),
                IdGianHang = reader.GetInt32("idGianHang"),
                Ten = reader["ten"]?.ToString() ?? string.Empty,
                DonGia = reader.GetDecimal("donGia"),
                TinhTrang = reader["tinhTrang"]?.ToString()
            };
        }

        private static void ValidateStoreRequest(UpsertStoreRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Ten))
                throw new ArgumentException("Ten gian hang khong duoc rong.");
            if (request.PhiHangThang < 0)
                throw new ArgumentException("Phi hang thang khong hop le.");
            NormalizeStoreStatus(request.TinhTrang);
        }

        private static void ValidateFoodRequest(UpsertFoodRequestDto request)
        {
            if (request.IdGianHang <= 0)
                throw new ArgumentException("Id gian hang khong hop le.");
            if (string.IsNullOrWhiteSpace(request.Ten))
                throw new ArgumentException("Ten mon an khong duoc rong.");
            if (request.DonGia < 0)
                throw new ArgumentException("Don gia khong hop le.");
            NormalizeFoodStatus(request.TinhTrang);
        }

        private static string NormalizeStoreStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status) || !StoreStatuses.Contains(status))
                throw new ArgumentException("Tinh trang gian hang khong hop le.");
            return status.Trim().ToLowerInvariant();
        }

        private static string NormalizeFoodStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status) || !FoodStatuses.Contains(status))
                throw new ArgumentException("Tinh trang mon an khong hop le.");
            return status.Trim().ToLowerInvariant();
        }

        private static string? NormalizeImagePathForWeb(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;

            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            var cleanPath = path.Trim().TrimStart('/');

            if (!cleanPath.StartsWith("images/", StringComparison.OrdinalIgnoreCase) &&
                !cleanPath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase) &&
                !cleanPath.StartsWith("content/", StringComparison.OrdinalIgnoreCase))
            {
                cleanPath = "images/" + cleanPath;
            }

            return "/" + cleanPath;
        }

        private static void DeleteManagedImageIfNeeded(string? existingPath, string webRoot, string replacementDbPath)
        {
            if (string.IsNullOrWhiteSpace(existingPath))
                return;

            var normalizedPath = existingPath.Trim().TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var replacementPath = replacementDbPath.Replace('/', Path.DirectorySeparatorChar);
            if (string.Equals(normalizedPath, replacementPath, StringComparison.OrdinalIgnoreCase))
                return;

            var managedRoot = Path.GetFullPath(Path.Combine(webRoot, "images", "stores"));
            var candidatePath = Path.GetFullPath(Path.Combine(webRoot, normalizedPath));

            if (!candidatePath.StartsWith(managedRoot, StringComparison.OrdinalIgnoreCase))
                return;

            if (File.Exists(candidatePath))
                File.Delete(candidatePath);
        }
    }
}
