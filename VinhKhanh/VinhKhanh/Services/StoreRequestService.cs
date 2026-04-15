using MySqlConnector;
using VinhKhanh.Data;
using VinhKhanh.Dtos;

namespace VinhKhanh.Services
{
    public class StoreRequestService
    {
        private static readonly HashSet<string> StoreStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "dang_hoat_dong",
            "tam_ngung",
            "dong_cua"
        };

        private static readonly HashSet<string> RequestStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "cho_duyet",
            "da_duyet",
            "tu_choi"
        };

        private readonly MySqlDbContext _db;

        public StoreRequestService(MySqlDbContext db)
        {
            _db = db;
        }

        public async Task<List<StoreRequestDto>> GetRequestsAsync(string? status = null)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            await EnsureStoreRequestTableAsync(conn);

            var normalizedStatus = NormalizeOptionalRequestStatus(status);

            var sql = @"
                SELECT
                    ycg.idYeuCau,
                    ycg.loaiYeuCau,
                    ycg.idChuQuanLy,
                    cql.idTaiKhoan AS idTaiKhoanChuQuanLy,
                    cql.hoTen AS hoTenChuQuanLy,
                    tk.username AS usernameChuQuanLy,
                    tk.email AS emailChuQuanLy,
                    ycg.tenGianHang,
                    ycg.diaChi,
                    ycg.moTa,
                    ycg.ngonNguMoTa,
                    ycg.lat,
                    ycg.lon,
                    ycg.phiHangThang,
                    ycg.tinhTrangDeXuat,
                    ycg.trangThaiYeuCau,
                    ycg.ghiChuXuLy,
                    ycg.idTaiKhoanXuLy,
                    COALESCE(adx.hoTen, tkx.username, tkx.email) AS tenNguoiXuLy,
                    ycg.idGianHang,
                    ycg.ngayTao,
                    ycg.thoiGianXuLy
                FROM yeucaugianhang ycg
                INNER JOIN chu_quan_ly cql ON cql.idChuQuanLy = ycg.idChuQuanLy
                INNER JOIN taikhoan tk ON tk.idTaiKhoan = cql.idTaiKhoan
                LEFT JOIN taikhoan tkx ON tkx.idTaiKhoan = ycg.idTaiKhoanXuLy
                LEFT JOIN admin adx ON adx.idTaiKhoan = tkx.idTaiKhoan";

            if (!string.IsNullOrWhiteSpace(normalizedStatus))
            {
                sql += " WHERE ycg.trangThaiYeuCau = @trangThaiYeuCau";
            }

            sql += @"
                ORDER BY
                    CASE ycg.trangThaiYeuCau
                        WHEN 'cho_duyet' THEN 0
                        WHEN 'da_duyet' THEN 1
                        ELSE 2
                    END,
                    ycg.ngayTao DESC,
                    ycg.idYeuCau DESC;";

            using var cmd = new MySqlCommand(sql, conn);
            if (!string.IsNullOrWhiteSpace(normalizedStatus))
            {
                cmd.Parameters.AddWithValue("@trangThaiYeuCau", normalizedStatus);
            }

            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<StoreRequestDto>();
            while (await reader.ReadAsync())
            {
                items.Add(MapStoreRequest(reader));
            }

            return items;
        }

        public async Task<List<StoreRequestDto>> GetRequestsByOwnerAsync(int idTaiKhoan)
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            await EnsureStoreRequestTableAsync(conn);

            const string sql = @"
                SELECT
                    ycg.idYeuCau,
                    ycg.loaiYeuCau,
                    ycg.idChuQuanLy,
                    cql.idTaiKhoan AS idTaiKhoanChuQuanLy,
                    cql.hoTen AS hoTenChuQuanLy,
                    tk.username AS usernameChuQuanLy,
                    tk.email AS emailChuQuanLy,
                    ycg.tenGianHang,
                    ycg.diaChi,
                    ycg.moTa,
                    ycg.ngonNguMoTa,
                    ycg.lat,
                    ycg.lon,
                    ycg.phiHangThang,
                    ycg.tinhTrangDeXuat,
                    ycg.trangThaiYeuCau,
                    ycg.ghiChuXuLy,
                    ycg.idTaiKhoanXuLy,
                    COALESCE(adx.hoTen, tkx.username, tkx.email) AS tenNguoiXuLy,
                    ycg.idGianHang,
                    ycg.ngayTao,
                    ycg.thoiGianXuLy
                FROM yeucaugianhang ycg
                INNER JOIN chu_quan_ly cql ON cql.idChuQuanLy = ycg.idChuQuanLy
                INNER JOIN taikhoan tk ON tk.idTaiKhoan = cql.idTaiKhoan
                LEFT JOIN taikhoan tkx ON tkx.idTaiKhoan = ycg.idTaiKhoanXuLy
                LEFT JOIN admin adx ON adx.idTaiKhoan = tkx.idTaiKhoan
                WHERE cql.idTaiKhoan = @idTaiKhoan
                ORDER BY ycg.ngayTao DESC, ycg.idYeuCau DESC;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idTaiKhoan", idTaiKhoan);

            using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<StoreRequestDto>();
            while (await reader.ReadAsync())
            {
                items.Add(MapStoreRequest(reader));
            }

            return items;
        }

        public async Task<StoreRequestDto> CreateRequestAsync(int idTaiKhoan, CreateStoreRequestDto request)
        {
            ValidateCreateRequest(request);

            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            await EnsureStoreRequestTableAsync(conn);

            var ownerId = await ResolveOwnerIdAsync(conn, idTaiKhoan);
            if (ownerId <= 0)
                throw new InvalidOperationException("Tai khoan khong co ho so chu quan ly.");

            const string sql = @"
                INSERT INTO yeucaugianhang
                (
                    loaiYeuCau,
                    idChuQuanLy,
                    tenGianHang,
                    diaChi,
                    moTa,
                    ngonNguMoTa,
                    lat,
                    lon,
                    phiHangThang,
                    tinhTrangDeXuat,
                    trangThaiYeuCau,
                    ngayTao
                )
                VALUES
                (
                    'them_gian_hang',
                    @idChuQuanLy,
                    @tenGianHang,
                    @diaChi,
                    @moTa,
                    @ngonNguMoTa,
                    @lat,
                    @lon,
                    @phiHangThang,
                    @tinhTrangDeXuat,
                    'cho_duyet',
                    NOW()
                );
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idChuQuanLy", ownerId);
            cmd.Parameters.AddWithValue("@tenGianHang", request.Ten.Trim());
            cmd.Parameters.AddWithValue("@diaChi", string.IsNullOrWhiteSpace(request.DiaChi) ? DBNull.Value : request.DiaChi.Trim());
            cmd.Parameters.AddWithValue("@moTa", string.IsNullOrWhiteSpace(request.MoTa) ? DBNull.Value : request.MoTa.Trim());
            cmd.Parameters.AddWithValue("@ngonNguMoTa", NormalizeLanguageCode(request.NgonNguMoTa));
            cmd.Parameters.AddWithValue("@lat", request.Lat.HasValue ? request.Lat.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@lon", request.Lon.HasValue ? request.Lon.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@phiHangThang", 0m);
            cmd.Parameters.AddWithValue("@tinhTrangDeXuat", NormalizeStoreStatus(request.TinhTrang));

            var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return await GetRequestByIdAsync(newId, conn)
                ?? throw new InvalidOperationException("Khong the tai yeu cau vua tao.");
        }

        public async Task<StoreRequestDto?> ReviewRequestAsync(int idYeuCau, int reviewerAccountId, ReviewStoreRequestDto request)
        {
            var requestStatus = NormalizeReviewDecision(request.TrangThaiYeuCau);
            var reviewerNote = string.IsNullOrWhiteSpace(request.GhiChuXuLy) ? null : request.GhiChuXuLy.Trim();
            var reviewedFee = NormalizeReviewedFee(request.PhiHangThang, requestStatus);

            using var conn = _db.GetConnection();
            await conn.OpenAsync();
            await EnsureStoreRequestTableAsync(conn);

            await using var transaction = await conn.BeginTransactionAsync();
            var currentRequest = await GetEditableRequestRowAsync(idYeuCau, conn, transaction);
            if (currentRequest == null)
            {
                await transaction.RollbackAsync();
                return null;
            }

            if (!string.Equals(currentRequest.TrangThaiYeuCau, "cho_duyet", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Yeu cau nay da duoc xu ly truoc do.");

            int? createdStoreId = null;
            if (string.Equals(requestStatus, "da_duyet", StringComparison.OrdinalIgnoreCase))
            {
                currentRequest.PhiHangThang = reviewedFee!.Value;
                createdStoreId = await CreateStoreFromRequestAsync(currentRequest, conn, transaction);

                if (!string.IsNullOrWhiteSpace(currentRequest.MoTa))
                {
                    await UpsertStoreDescriptionAsync(
                        createdStoreId.Value,
                        currentRequest.TenGianHang,
                        currentRequest.MoTa!,
                        currentRequest.NgonNguMoTa,
                        conn,
                        transaction
                    );
                }
            }

            const string updateSql = @"
                UPDATE yeucaugianhang
                SET trangThaiYeuCau = @trangThaiYeuCau,
                    ghiChuXuLy = @ghiChuXuLy,
                    idTaiKhoanXuLy = @idTaiKhoanXuLy,
                    phiHangThang = COALESCE(@phiHangThang, phiHangThang),
                    idGianHang = COALESCE(@idGianHang, idGianHang),
                    thoiGianXuLy = NOW()
                WHERE idYeuCau = @idYeuCau;";

            using (var updateCmd = new MySqlCommand(updateSql, conn, transaction))
            {
                updateCmd.Parameters.AddWithValue("@trangThaiYeuCau", requestStatus);
                updateCmd.Parameters.AddWithValue("@ghiChuXuLy", (object?)reviewerNote ?? DBNull.Value);
                updateCmd.Parameters.AddWithValue("@idTaiKhoanXuLy", reviewerAccountId);
                updateCmd.Parameters.AddWithValue("@phiHangThang", reviewedFee.HasValue ? reviewedFee.Value : DBNull.Value);
                updateCmd.Parameters.AddWithValue("@idGianHang", createdStoreId.HasValue ? createdStoreId.Value : DBNull.Value);
                updateCmd.Parameters.AddWithValue("@idYeuCau", idYeuCau);
                await updateCmd.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();
            return await GetRequestByIdAsync(idYeuCau, conn);
        }

        private static StoreRequestDto MapStoreRequest(MySqlDataReader reader)
        {
            return new StoreRequestDto
            {
                IdYeuCau = reader.GetInt32("idYeuCau"),
                LoaiYeuCau = reader["loaiYeuCau"]?.ToString() ?? "them_gian_hang",
                IdChuQuanLy = reader.GetInt32("idChuQuanLy"),
                IdTaiKhoanChuQuanLy = reader.GetInt32("idTaiKhoanChuQuanLy"),
                HoTenChuQuanLy = reader["hoTenChuQuanLy"] == DBNull.Value ? null : reader["hoTenChuQuanLy"]?.ToString(),
                UsernameChuQuanLy = reader["usernameChuQuanLy"] == DBNull.Value ? null : reader["usernameChuQuanLy"]?.ToString(),
                EmailChuQuanLy = reader["emailChuQuanLy"] == DBNull.Value ? null : reader["emailChuQuanLy"]?.ToString(),
                TenGianHang = reader["tenGianHang"]?.ToString() ?? string.Empty,
                DiaChi = reader["diaChi"] == DBNull.Value ? null : reader["diaChi"]?.ToString(),
                MoTa = reader["moTa"] == DBNull.Value ? null : reader["moTa"]?.ToString(),
                NgonNguMoTa = reader["ngonNguMoTa"]?.ToString() ?? "vi",
                Lat = reader["lat"] == DBNull.Value ? null : Convert.ToDouble(reader["lat"]),
                Lon = reader["lon"] == DBNull.Value ? null : Convert.ToDouble(reader["lon"]),
                PhiHangThang = reader["phiHangThang"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["phiHangThang"]),
                TinhTrangDeXuat = reader["tinhTrangDeXuat"]?.ToString() ?? "dang_hoat_dong",
                TrangThaiYeuCau = reader["trangThaiYeuCau"]?.ToString() ?? "cho_duyet",
                GhiChuXuLy = reader["ghiChuXuLy"] == DBNull.Value ? null : reader["ghiChuXuLy"]?.ToString(),
                IdTaiKhoanXuLy = reader["idTaiKhoanXuLy"] == DBNull.Value ? null : Convert.ToInt32(reader["idTaiKhoanXuLy"]),
                TenNguoiXuLy = reader["tenNguoiXuLy"] == DBNull.Value ? null : reader["tenNguoiXuLy"]?.ToString(),
                IdGianHang = reader["idGianHang"] == DBNull.Value ? null : Convert.ToInt32(reader["idGianHang"]),
                NgayTao = Convert.ToDateTime(reader["ngayTao"]),
                ThoiGianXuLy = reader["thoiGianXuLy"] == DBNull.Value ? null : Convert.ToDateTime(reader["thoiGianXuLy"])
            };
        }

        private async Task<StoreRequestDto?> GetRequestByIdAsync(int idYeuCau, MySqlConnection conn)
        {
            const string sql = @"
                SELECT
                    ycg.idYeuCau,
                    ycg.loaiYeuCau,
                    ycg.idChuQuanLy,
                    cql.idTaiKhoan AS idTaiKhoanChuQuanLy,
                    cql.hoTen AS hoTenChuQuanLy,
                    tk.username AS usernameChuQuanLy,
                    tk.email AS emailChuQuanLy,
                    ycg.tenGianHang,
                    ycg.diaChi,
                    ycg.moTa,
                    ycg.ngonNguMoTa,
                    ycg.lat,
                    ycg.lon,
                    ycg.phiHangThang,
                    ycg.tinhTrangDeXuat,
                    ycg.trangThaiYeuCau,
                    ycg.ghiChuXuLy,
                    ycg.idTaiKhoanXuLy,
                    COALESCE(adx.hoTen, tkx.username, tkx.email) AS tenNguoiXuLy,
                    ycg.idGianHang,
                    ycg.ngayTao,
                    ycg.thoiGianXuLy
                FROM yeucaugianhang ycg
                INNER JOIN chu_quan_ly cql ON cql.idChuQuanLy = ycg.idChuQuanLy
                INNER JOIN taikhoan tk ON tk.idTaiKhoan = cql.idTaiKhoan
                LEFT JOIN taikhoan tkx ON tkx.idTaiKhoan = ycg.idTaiKhoanXuLy
                LEFT JOIN admin adx ON adx.idTaiKhoan = tkx.idTaiKhoan
                WHERE ycg.idYeuCau = @idYeuCau
                LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idYeuCau", idYeuCau);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return MapStoreRequest(reader);
        }

        private async Task<EditableStoreRequestRow?> GetEditableRequestRowAsync(int idYeuCau, MySqlConnection conn, MySqlTransaction transaction)
        {
            const string sql = @"
                SELECT
                    idYeuCau,
                    idChuQuanLy,
                    tenGianHang,
                    diaChi,
                    moTa,
                    ngonNguMoTa,
                    lat,
                    lon,
                    phiHangThang,
                    tinhTrangDeXuat,
                    trangThaiYeuCau
                FROM yeucaugianhang
                WHERE idYeuCau = @idYeuCau
                LIMIT 1
                FOR UPDATE;";

            using var cmd = new MySqlCommand(sql, conn, transaction);
            cmd.Parameters.AddWithValue("@idYeuCau", idYeuCau);

            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            return new EditableStoreRequestRow
            {
                IdYeuCau = reader.GetInt32("idYeuCau"),
                IdChuQuanLy = reader.GetInt32("idChuQuanLy"),
                TenGianHang = reader["tenGianHang"]?.ToString() ?? string.Empty,
                DiaChi = reader["diaChi"] == DBNull.Value ? null : reader["diaChi"]?.ToString(),
                MoTa = reader["moTa"] == DBNull.Value ? null : reader["moTa"]?.ToString(),
                NgonNguMoTa = reader["ngonNguMoTa"]?.ToString() ?? "vi",
                Lat = reader["lat"] == DBNull.Value ? null : Convert.ToDouble(reader["lat"]),
                Lon = reader["lon"] == DBNull.Value ? null : Convert.ToDouble(reader["lon"]),
                PhiHangThang = reader["phiHangThang"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["phiHangThang"]),
                TinhTrangDeXuat = reader["tinhTrangDeXuat"]?.ToString() ?? "dang_hoat_dong",
                TrangThaiYeuCau = reader["trangThaiYeuCau"]?.ToString() ?? "cho_duyet"
            };
        }

        private async Task<int> ResolveOwnerIdAsync(MySqlConnection conn, int idTaiKhoan)
        {
            const string sql = @"
                SELECT idChuQuanLy
                FROM chu_quan_ly
                WHERE idTaiKhoan = @idTaiKhoan
                LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@idTaiKhoan", idTaiKhoan);
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        private async Task<int> CreateStoreFromRequestAsync(EditableStoreRequestRow request, MySqlConnection conn, MySqlTransaction transaction)
        {
            const string sql = @"
                INSERT INTO gianhang
                (
                    idChuQuanLy,
                    ten,
                    diaChi,
                    lat,
                    lon,
                    tinhTrang,
                    phiHangThang,
                    ngayDangKy,
                    thoiGianCapNhat
                )
                VALUES
                (
                    @idChuQuanLy,
                    @ten,
                    @diaChi,
                    @lat,
                    @lon,
                    @tinhTrang,
                    @phiHangThang,
                    NOW(),
                    NOW()
                );
                SELECT LAST_INSERT_ID();";

            using var cmd = new MySqlCommand(sql, conn, transaction);
            cmd.Parameters.AddWithValue("@idChuQuanLy", request.IdChuQuanLy);
            cmd.Parameters.AddWithValue("@ten", request.TenGianHang);
            cmd.Parameters.AddWithValue("@diaChi", (object?)request.DiaChi ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@lat", request.Lat.HasValue ? request.Lat.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@lon", request.Lon.HasValue ? request.Lon.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@tinhTrang", NormalizeStoreStatus(request.TinhTrangDeXuat));
            cmd.Parameters.AddWithValue("@phiHangThang", request.PhiHangThang);

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        private static async Task UpsertStoreDescriptionAsync(
            int idGianHang,
            string tenGianHang,
            string moTa,
            string ngonNguMoTa,
            MySqlConnection conn,
            MySqlTransaction transaction)
        {
            var languageCode = NormalizeLanguageCode(ngonNguMoTa);
            var languageId = await ResolveLanguageIdAsync(languageCode, conn, transaction)
                ?? await ResolveLanguageIdAsync("vi", conn, transaction);

            if (!languageId.HasValue)
                return;

            const string sql = @"
                INSERT INTO gianhangngonngu (idGianHang, idNgonNgu, ten, audioURL, moTa)
                VALUES (@idGianHang, @idNgonNgu, @ten, NULL, @moTa)
                ON DUPLICATE KEY UPDATE
                    ten = VALUES(ten),
                    moTa = VALUES(moTa),
                    audioURL = NULL;";

            using var cmd = new MySqlCommand(sql, conn, transaction);
            cmd.Parameters.AddWithValue("@idGianHang", idGianHang);
            cmd.Parameters.AddWithValue("@idNgonNgu", languageId.Value);
            cmd.Parameters.AddWithValue("@ten", tenGianHang);
            cmd.Parameters.AddWithValue("@moTa", moTa);
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task<int?> ResolveLanguageIdAsync(string languageCode, MySqlConnection conn, MySqlTransaction transaction)
        {
            const string sql = @"
                SELECT idNgonNgu
                FROM ngonngu
                WHERE maNgonNgu = @languageCode
                LIMIT 1;";

            using var cmd = new MySqlCommand(sql, conn, transaction);
            cmd.Parameters.AddWithValue("@languageCode", languageCode);
            var result = await cmd.ExecuteScalarAsync();
            return result == null || result == DBNull.Value ? null : Convert.ToInt32(result);
        }

        private static void ValidateCreateRequest(CreateStoreRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Ten))
                throw new ArgumentException("Ten gian hang khong duoc rong.");

            if (request.PhiHangThang < 0)
                throw new ArgumentException("Phi hang thang khong hop le.");

            NormalizeStoreStatus(request.TinhTrang);
            NormalizeLanguageCode(request.NgonNguMoTa);
        }

        private static string NormalizeStoreStatus(string? status)
        {
            var normalized = string.IsNullOrWhiteSpace(status)
                ? "dang_hoat_dong"
                : status.Trim().ToLowerInvariant();

            if (!StoreStatuses.Contains(normalized))
                throw new ArgumentException("Tinh trang gian hang khong hop le.");

            return normalized;
        }

        private static string NormalizeOptionalRequestStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status) || string.Equals(status.Trim(), "all", StringComparison.OrdinalIgnoreCase))
                return string.Empty;

            var normalized = status.Trim().ToLowerInvariant();
            if (!RequestStatuses.Contains(normalized))
                throw new ArgumentException("Trang thai yeu cau khong hop le.");

            return normalized;
        }

        private static string NormalizeReviewDecision(string? status)
        {
            var normalized = string.IsNullOrWhiteSpace(status)
                ? "da_duyet"
                : status.Trim().ToLowerInvariant();

            if (!string.Equals(normalized, "da_duyet", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(normalized, "tu_choi", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Chi ho tro duyet hoac tu choi yeu cau.");
            }

            return normalized;
        }

        private static decimal? NormalizeReviewedFee(decimal? phiHangThang, string requestStatus)
        {
            if (!string.Equals(requestStatus, "da_duyet", StringComparison.OrdinalIgnoreCase))
                return phiHangThang;

            if (!phiHangThang.HasValue)
                throw new ArgumentException("Admin phai nhap phi hang thang truoc khi phe duyet.");

            if (phiHangThang.Value < 0)
                throw new ArgumentException("Phi hang thang khong hop le.");

            return phiHangThang.Value;
        }

        private static string NormalizeLanguageCode(string? languageCode)
        {
            return string.IsNullOrWhiteSpace(languageCode)
                ? "vi"
                : languageCode.Trim().ToLowerInvariant();
        }

        private static async Task EnsureStoreRequestTableAsync(MySqlConnection conn)
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS yeucaugianhang (
                    idYeuCau INT NOT NULL AUTO_INCREMENT,
                    loaiYeuCau ENUM('them_gian_hang') NOT NULL DEFAULT 'them_gian_hang',
                    idChuQuanLy INT NOT NULL,
                    tenGianHang VARCHAR(150) NOT NULL,
                    diaChi VARCHAR(255) DEFAULT NULL,
                    moTa TEXT DEFAULT NULL,
                    ngonNguMoTa VARCHAR(10) NOT NULL DEFAULT 'vi',
                    lat DECIMAL(10,7) DEFAULT NULL,
                    lon DECIMAL(10,7) DEFAULT NULL,
                    phiHangThang DECIMAL(12,2) NOT NULL DEFAULT 0.00,
                    tinhTrangDeXuat ENUM('dang_hoat_dong','tam_ngung','dong_cua') NOT NULL DEFAULT 'dang_hoat_dong',
                    trangThaiYeuCau ENUM('cho_duyet','da_duyet','tu_choi') NOT NULL DEFAULT 'cho_duyet',
                    ghiChuXuLy TEXT DEFAULT NULL,
                    idTaiKhoanXuLy INT DEFAULT NULL,
                    idGianHang INT DEFAULT NULL,
                    ngayTao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP(),
                    thoiGianXuLy DATETIME DEFAULT NULL,
                    PRIMARY KEY (idYeuCau),
                    KEY idx_yeucaugianhang_owner (idChuQuanLy),
                    KEY idx_yeucaugianhang_status (trangThaiYeuCau),
                    KEY idx_yeucaugianhang_store (idGianHang),
                    CONSTRAINT fk_yeucaugianhang_owner
                        FOREIGN KEY (idChuQuanLy) REFERENCES chu_quan_ly (idChuQuanLy)
                        ON DELETE CASCADE ON UPDATE CASCADE,
                    CONSTRAINT fk_yeucaugianhang_reviewer
                        FOREIGN KEY (idTaiKhoanXuLy) REFERENCES taikhoan (idTaiKhoan)
                        ON DELETE SET NULL ON UPDATE CASCADE,
                    CONSTRAINT fk_yeucaugianhang_store
                        FOREIGN KEY (idGianHang) REFERENCES gianhang (idGianHang)
                        ON DELETE SET NULL ON UPDATE CASCADE
                ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;";

            using var cmd = new MySqlCommand(sql, conn);
            await cmd.ExecuteNonQueryAsync();
        }

        private sealed class EditableStoreRequestRow
        {
            public int IdYeuCau { get; set; }
            public int IdChuQuanLy { get; set; }
            public string TenGianHang { get; set; } = string.Empty;
            public string? DiaChi { get; set; }
            public string? MoTa { get; set; }
            public string NgonNguMoTa { get; set; } = "vi";
            public double? Lat { get; set; }
            public double? Lon { get; set; }
            public decimal PhiHangThang { get; set; }
            public string TinhTrangDeXuat { get; set; } = "dang_hoat_dong";
            public string TrangThaiYeuCau { get; set; } = "cho_duyet";
        }
    }
}
