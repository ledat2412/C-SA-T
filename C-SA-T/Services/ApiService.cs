using System.Net.Http.Json;
using MauiApp1.Models;
using Microsoft.Maui.Storage;

namespace MauiApp1.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ClientDeviceIdentityService _clientDeviceIdentityService;

        public ApiService(HttpClient httpClient, ClientDeviceIdentityService clientDeviceIdentityService)
        {
            _httpClient = httpClient;
            _clientDeviceIdentityService = clientDeviceIdentityService;
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

        public async Task<QrScanResult> ScanQrAsync(string qrRaw, int? idGoi = null)
        {
            if (string.IsNullOrWhiteSpace(qrRaw))
            {
                return new QrScanResult
                {
                    Success = false,
                    Message = "QR rong, vui long thu lai."
                };
            }

            var accessToken = ExtractAccessToken(qrRaw);
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                var validation = await RecoverAccessAsync(accessToken, qrRaw);
                return new QrScanResult
                {
                    Success = validation.Success,
                    Message = validation.Success
                        ? "Khoi phuc token tu QR thanh cong."
                        : validation.Message,
                    AccessToken = accessToken,
                    MaThietBi = validation.MaThietBi,
                    HetHanLuc = validation.HetHanLuc,
                    BatDauLuc = validation.BatDauLuc,
                    TrangThai = validation.TrangThai,
                    IdGoi = validation.IdGoi,
                    TenGoi = validation.TenGoi,
                    SoNgayHieuLuc = validation.SoNgayHieuLuc,
                    EmailStatusMessage = validation.Message
                };
            }

            var maThietBi = ExtractDeviceCode(qrRaw);
            if (string.IsNullOrWhiteSpace(maThietBi))
            {
                return new QrScanResult
                {
                    Success = false,
                    Message = "Khong doc duoc ma thiet bi tu QR."
                };
            }

            var response = await _httpClient.PostAsJsonAsync("api/access/scan", new
            {
                MaThietBi = maThietBi,
                QrRaw = qrRaw,
                IdGoi = idGoi
            });

            QrScanResult? result = null;

            try
            {
                result = await response.Content.ReadFromJsonAsync<QrScanResult>();
            }
            catch
            {
            }

            if (result != null)
            {
                if (result.Success && !string.IsNullOrWhiteSpace(result.AccessToken))
                {
                    Preferences.Set("access_token", result.AccessToken);
                    Preferences.Set("access_device_code", result.MaThietBi ?? string.Empty);
                }

                return result;
            }

            return new QrScanResult
            {
                Success = response.IsSuccessStatusCode,
                Message = response.ReasonPhrase ?? "Khong xu ly duoc phan hoi quet QR.",
                MaThietBi = maThietBi
            };
        }

        public async Task<PackageAccessRegistrationResult> RegisterPackageAccessAsync(string email, int idGoi, bool bypassPayment)
        {
            var response = await _httpClient.PostAsJsonAsync("api/access/package/register", new
            {
                Email = email,
                IdGoi = idGoi,
                BypassPayment = bypassPayment,
                ClientDeviceId = _clientDeviceIdentityService.GetOrCreateClientDeviceId()
            });

            PackageAccessRegistrationResult? result = null;

            try
            {
                result = await response.Content.ReadFromJsonAsync<PackageAccessRegistrationResult>();
            }
            catch
            {
            }

            return result ?? new PackageAccessRegistrationResult
            {
                Success = response.IsSuccessStatusCode,
                Message = response.ReasonPhrase ?? "Khong dang ky duoc goi dich vu."
            };
        }

        public async Task<ValidateAccessResult> ValidateAccessAsync(string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new ValidateAccessResult
                {
                    IsValid = false,
                    Message = "Thieu access token."
                };
            }

            var clientDeviceId = _clientDeviceIdentityService.GetOrCreateClientDeviceId();
            var url = $"api/access/validate?accessToken={Uri.EscapeDataString(accessToken)}&clientDeviceId={Uri.EscapeDataString(clientDeviceId)}";
            var response = await _httpClient.GetAsync(url);

            ValidateAccessResult? result = null;

            try
            {
                result = await response.Content.ReadFromJsonAsync<ValidateAccessResult>();
            }
            catch
            {
            }

            return result ?? new ValidateAccessResult
            {
                IsValid = false,
                Message = response.ReasonPhrase ?? "Khong validate duoc access token."
            };
        }

        public async Task<RecoverAccessResult> RecoverAccessAsync(string accessToken, string? qrRaw = null)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return new RecoverAccessResult
                {
                    Success = false,
                    Message = "Thieu access token de khoi phuc."
                };
            }

            var response = await _httpClient.PostAsJsonAsync("api/access/recover", new
            {
                AccessToken = accessToken,
                ClientDeviceId = _clientDeviceIdentityService.GetOrCreateClientDeviceId(),
                QrRaw = qrRaw ?? string.Empty
            });

            RecoverAccessResult? result = null;

            try
            {
                result = await response.Content.ReadFromJsonAsync<RecoverAccessResult>();
            }
            catch
            {
            }

            return result ?? new RecoverAccessResult
            {
                Success = response.IsSuccessStatusCode,
                Message = response.ReasonPhrase ?? "Khong khoi phuc duoc access token."
            };
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

        private static string? ExtractDeviceCode(string rawValue)
        {
            var normalized = rawValue.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                return null;

            if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
            {
                var query = ParseQueryString(uri.Query);
                foreach (var key in new[] { "maThietBi", "mathietbi", "deviceCode", "device", "code" })
                {
                    if (query.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                        return value.Trim();
                }

                var lastSegment = uri.Segments.LastOrDefault()?.Trim('/');
                if (!string.IsNullOrWhiteSpace(lastSegment))
                    return lastSegment;
            }

            return normalized;
        }

        private static string? ExtractAccessToken(string rawValue)
        {
            var normalized = rawValue.Trim();
            if (string.IsNullOrWhiteSpace(normalized))
                return null;

            if (normalized.StartsWith("TEST-", StringComparison.OrdinalIgnoreCase))
                return normalized;

            if (Uri.TryCreate(normalized, UriKind.Absolute, out var uri))
            {
                var query = ParseQueryString(uri.Query);
                foreach (var key in new[] { "accessToken", "access_token", "token" })
                {
                    if (query.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                        return value.Trim();
                }
            }

            return null;
        }

        private static Dictionary<string, string> ParseQueryString(string query)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(query))
                return result;

            foreach (var part in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var pair = part.Split('=', 2);
                var key = Uri.UnescapeDataString(pair[0]);
                var value = pair.Length > 1 ? Uri.UnescapeDataString(pair[1]) : string.Empty;
                if (!string.IsNullOrWhiteSpace(key))
                    result[key] = value;
            }

            return result;
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

    public class QrScanResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? MaThietBi { get; set; }
        public string? AccessToken { get; set; }
        public DateTime? BatDauLuc { get; set; }
        public DateTime? HetHanLuc { get; set; }
        public string? TrangThai { get; set; }
        public int? IdGoi { get; set; }
        public string? TenGoi { get; set; }
        public int? SoNgayHieuLuc { get; set; }
        public string? RecoveryQrPayload { get; set; }
        public bool EmailSent { get; set; }
        public string? EmailStatusMessage { get; set; }
        public int? IdHoaDon { get; set; }
    }

    public class ValidateAccessResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? MaThietBi { get; set; }
        public DateTime? BatDauLuc { get; set; }
        public DateTime? HetHanLuc { get; set; }
        public string? TrangThai { get; set; }
    }

    public class RecoverAccessResult : QrScanResult
    {
    }

    public class PackageAccessRegistrationResult : QrScanResult
    {
    }
}
