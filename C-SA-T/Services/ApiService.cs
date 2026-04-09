using System.Net.Http.Json;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AppDataResponse> GetAppDataAsync(string lang = "vi")
        {
            var url = $"api/gianhang/appdata?lang={lang}";
            var result = await _httpClient.GetFromJsonAsync<AppDataResponse>(url);
            return result ?? new AppDataResponse();
        }

        public async Task<LoginResult> LoginAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
            {
                Username = username,
                MatKhau = password
            });

            LoginResult? result = null;

            try
            {
                result = await response.Content.ReadFromJsonAsync<LoginResult>();
            }
            catch
            {
            }

            if (result != null)
                return result;

            return new LoginResult
            {
                Success = response.IsSuccessStatusCode,
                Message = response.ReasonPhrase ?? "Đăng nhập thất bại."
            };
        }

        public async Task<List<GianHang>> GetAllGianHangsAsync(string lang = "vi")
        {
            var data = await GetAppDataAsync(lang);
            return data.GianHangs;
        }

        public async Task<GianHang?> GetGianHangDetailAsync(int idGianHang, string lang = "vi")
        {
            var data = await GetAppDataAsync(lang);
            return data.GianHangs.FirstOrDefault(x => x.IdGianHang == idGianHang);
        }

        public async Task<List<GianHang>> GetNearbyGianHangsAsync(
            double lat,
            double lon,
            string lang = "vi",
            double radiusMeters = 100)
        {
            var data = await GetAppDataAsync(lang);

            return data.GianHangs
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
            const double R = 6371000.0;
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
    }

    public class LoginResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? IdTaiKhoan { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? LoaiTaiKhoan { get; set; }
        public int? IdAdmin { get; set; }
        public int? IdChuQuanLy { get; set; }
        public string? HoTen { get; set; }
    }
}
