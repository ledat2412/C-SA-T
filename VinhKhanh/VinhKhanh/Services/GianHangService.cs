using MySqlConnector;
using VinhKhanh.Data;
using VinhKhanh.Dtos;

namespace VinhKhanh.Services
{
    public class GianHangService
    {
        private readonly MySqlDbContext _db;
        private readonly GoogleTtsService _ttsService;

        public GianHangService(MySqlDbContext db, GoogleTtsService ttsService)
        {
            _db = db;
            _ttsService = ttsService;
        }

        public async Task<AppDataDto> GetAppDataAsync(string lang = "vi")
        {
            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            var gianHangs = await GetAllGianHangDetailsAsync(conn, lang);
            var hinhAnhGianHang = await GetAllHinhAnhGianHangAsync(conn);
            var monAns = await GetAllMonAnAsync(conn, lang);

            foreach (var gianHang in gianHangs)
            {
                if (hinhAnhGianHang.TryGetValue(gianHang.IdGianHang, out var images))
                {
                    gianHang.HinhAnhPhu = images;
                    gianHang.HinhAnhChinh = images.FirstOrDefault();
                }

                gianHang.MonAns = monAns
                    .Where(x => x.IdGianHang == gianHang.IdGianHang)
                    .ToList();
            }

            return new AppDataDto
            {
                GianHangs = gianHangs
            };
        }

        private async Task<List<GianHangDetailDto>> GetAllGianHangDetailsAsync(MySqlConnection conn, string lang)
        {
            var list = new List<GianHangDetailDto>();

            const string sql = @"
                SELECT 
                    gh.idGianHang,
                    COALESCE(ghnn.ten, gh.ten) AS ten,
                    gh.diaChi,
                    ghnn.moTa,
                    ghnn.audioURL,
                    gh.lat,
                    gh.lon,
                    gh.tinhTrang
                FROM gianhang gh
                LEFT JOIN ngonngu nn 
                    ON nn.maNgonNgu = @lang
                LEFT JOIN gianhangngonngu ghnn 
                    ON ghnn.idGianHang = gh.idGianHang
                    AND ghnn.idNgonNgu = nn.idNgonNgu
                WHERE gh.tinhTrang = 'dang_hoat_dong'
                ORDER BY gh.idGianHang;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@lang", lang);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new GianHangDetailDto
                {
                    IdGianHang = reader.GetInt32("idGianHang"),
                    Ten = reader["ten"]?.ToString() ?? "",
                    DiaChi = reader["diaChi"]?.ToString(),
                    MoTa = reader["moTa"]?.ToString(),
                    AudioURL = reader["audioURL"]?.ToString(),
                    Lat = reader["lat"] == DBNull.Value ? null : Convert.ToDouble(reader["lat"]),
                    Lon = reader["lon"] == DBNull.Value ? null : Convert.ToDouble(reader["lon"]),
                    TinhTrang = reader["tinhTrang"]?.ToString()
                });
            }

            return list;
        }

        private async Task<Dictionary<int, List<string>>> GetAllHinhAnhGianHangAsync(MySqlConnection conn)
        {
            var dict = new Dictionary<int, List<string>>();

            const string sql = @"
                SELECT idGianHang, duongDan
                FROM hinhanhgianhang
                ORDER BY idGianHang, idHinhAnh;";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int idGianHang = reader.GetInt32("idGianHang");
                string duongDan = reader["duongDan"]?.ToString() ?? "";

                if (!dict.ContainsKey(idGianHang))
                    dict[idGianHang] = new List<string>();

                if (!string.IsNullOrWhiteSpace(duongDan))
                {
                    var normalizedPath = NormalizeImagePathForWeb(duongDan);
                    dict[idGianHang].Add(normalizedPath);
                }
            }

            return dict;
        }

        private static string NormalizeImagePathForWeb(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;

            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            var cleanPath = path.TrimStart('/');

            if (!cleanPath.StartsWith("images/", StringComparison.OrdinalIgnoreCase) &&
                !cleanPath.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase) &&
                !cleanPath.StartsWith("content/", StringComparison.OrdinalIgnoreCase))
            {
                cleanPath = "images/" + cleanPath;
            }

            return "/" + cleanPath;
        }

        private async Task<List<MonAnDto>> GetAllMonAnAsync(MySqlConnection conn, string lang)
        {
            var list = new List<MonAnDto>();

            const string sql = @"
                SELECT 
                    ma.idMonAn,
                    ma.idGianHang,
                    COALESCE(mann.ten, ma.ten) AS ten,
                    ma.donGia,
                    mann.moTa,
                    ma.tinhTrang,
                    MIN(ham.duongDan) AS hinhAnh
                FROM monan ma
                LEFT JOIN ngonngu nn
                    ON nn.maNgonNgu = @lang
                LEFT JOIN monanngonngu mann
                    ON mann.idMonAn = ma.idMonAn
                    AND mann.idNgonNgu = nn.idNgonNgu
                LEFT JOIN hinhanhmonan ham
                    ON ham.idMonAn = ma.idMonAn
                GROUP BY 
                    ma.idMonAn, ma.idGianHang, ma.ten, mann.ten, ma.donGia, mann.moTa, ma.tinhTrang
                ORDER BY ma.idGianHang, ma.idMonAn;";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@lang", lang);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new MonAnDto
                {
                    IdMonAn = reader.GetInt32("idMonAn"),
                    IdGianHang = reader.GetInt32("idGianHang"),
                    Ten = reader["ten"]?.ToString() ?? "",
                    DonGia = reader.GetDecimal("donGia"),
                    MoTa = reader["moTa"]?.ToString(),
                    TinhTrang = reader["tinhTrang"]?.ToString(),
                    HinhAnh = reader["hinhAnh"]?.ToString()
                });
            }

            return list;
        }

        public async Task<List<GianHangDto>> GetAllAsync(string lang = "vi")
        {
            var data = await GetAppDataAsync(lang);

            return data.GianHangs.Select(x => new GianHangDto
            {
                IdGianHang = x.IdGianHang,
                Ten = x.Ten,
                DiaChi = x.DiaChi,
                MoTa = x.MoTa,
                AudioURL = x.AudioURL,
                HinhAnh = x.HinhAnhChinh,
                Lat = x.Lat,
                Lon = x.Lon,
                TinhTrang = x.TinhTrang
            }).ToList();
        }

        public async Task<GianHangDetailDto?> GetByIdAsync(int idGianHang, string lang = "vi")
        {
            var data = await GetAppDataAsync(lang);
            return data.GianHangs.FirstOrDefault(x => x.IdGianHang == idGianHang);
        }

        public async Task<List<GianHangDto>> GetNearbyAsync(double lat, double lon, double radiusMeters = 100, string lang = "vi")
        {
            var all = await GetAllAsync(lang);

            return all
                .Where(x => x.Lat.HasValue && x.Lon.HasValue)
                .Select(x => new
                {
                    GianHang = x,
                    Distance = CalculateDistanceMeters(lat, lon, x.Lat!.Value, x.Lon!.Value)
                })
                .Where(x => x.Distance <= radiusMeters)
                .OrderBy(x => x.Distance)
                .Select(x => x.GianHang)
                .ToList();
        }

        private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000;
            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public async Task<object?> GenerateAudioFromMoTaAsync(int idGianHang, string languageCode = "vi")
        {
            var normalizedLanguageCode = NormalizeLanguageCode(languageCode);

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string selectSql = @"
                SELECT 
                    ghnn.id,
                    ghnn.idGianHang,
                    nn.maNgonNgu,
                    ghnn.ten,
                    ghnn.moTa,
                    ghnn.audioURL
                FROM gianhangngonngu ghnn
                INNER JOIN ngonngu nn ON ghnn.idNgonNgu = nn.idNgonNgu
                WHERE ghnn.idGianHang = @idGianHang
                  AND nn.maNgonNgu = @languageCode
                LIMIT 1;";

            using var cmd = new MySqlCommand(selectSql, conn);
            cmd.Parameters.AddWithValue("@idGianHang", idGianHang);
            cmd.Parameters.AddWithValue("@languageCode", normalizedLanguageCode);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            var ten = reader["ten"]?.ToString();
            var moTa = reader["moTa"]?.ToString();
            var oldAudioUrl = reader["audioURL"]?.ToString();

            await reader.CloseAsync();

            if (string.IsNullOrWhiteSpace(moTa))
                return null;

            if (!string.IsNullOrWhiteSpace(oldAudioUrl) && _ttsService.AudioPathExists(oldAudioUrl))
            {
                return new
                {
                    idGianHang,
                    languageCode = normalizedLanguageCode,
                    ten,
                    moTa,
                    audioURL = oldAudioUrl,
                    isCached = true
                };
            }

            var fileName = $"gianhang_{idGianHang}_{normalizedLanguageCode}.mp3";
            var generatedUrl = await _ttsService.GenerateSpeechAsync(moTa, fileName, normalizedLanguageCode);
            var dbAudioUrl = generatedUrl.TrimStart('/');

            const string updateSql = @"
                UPDATE gianhangngonngu ghnn
                INNER JOIN ngonngu nn ON ghnn.idNgonNgu = nn.idNgonNgu
                SET ghnn.audioURL = @audioURL
                WHERE ghnn.idGianHang = @idGianHang
                  AND nn.maNgonNgu = @languageCode;";

            using var updateCmd = new MySqlCommand(updateSql, conn);
            updateCmd.Parameters.AddWithValue("@audioURL", dbAudioUrl);
            updateCmd.Parameters.AddWithValue("@idGianHang", idGianHang);
            updateCmd.Parameters.AddWithValue("@languageCode", normalizedLanguageCode);
            await updateCmd.ExecuteNonQueryAsync();

            return new
            {
                idGianHang,
                languageCode = normalizedLanguageCode,
                ten,
                moTa,
                audioURL = dbAudioUrl,
                isCached = false
            };
        }

        public async Task<object?> UpdateMoTaAndGenerateAudioAsync(int idGianHang, string languageCode, string moTa)
        {
            var normalizedLanguageCode = NormalizeLanguageCode(languageCode);

            using var conn = _db.GetConnection();
            await conn.OpenAsync();

            const string updateSql = @"
                UPDATE gianhangngonngu ghnn
                INNER JOIN ngonngu nn ON ghnn.idNgonNgu = nn.idNgonNgu
                SET ghnn.moTa = @moTa,
                    ghnn.audioURL = NULL
                WHERE ghnn.idGianHang = @idGianHang
                  AND nn.maNgonNgu = @languageCode;";

            using var updateCmd = new MySqlCommand(updateSql, conn);
            updateCmd.Parameters.AddWithValue("@moTa", moTa);
            updateCmd.Parameters.AddWithValue("@idGianHang", idGianHang);
            updateCmd.Parameters.AddWithValue("@languageCode", normalizedLanguageCode);

            var rows = await updateCmd.ExecuteNonQueryAsync();
            if (rows <= 0)
                return null;

            return await GenerateAudioFromMoTaAsync(idGianHang, normalizedLanguageCode);
        }

        private static string NormalizeLanguageCode(string? languageCode)
        {
            return string.IsNullOrWhiteSpace(languageCode)
                ? "vi"
                : languageCode.Trim().ToLowerInvariant();
        }
    }
}
