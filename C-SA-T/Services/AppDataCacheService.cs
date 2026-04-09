using System.Text.Json;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    public class AppDataCacheService
    {
        private readonly ApiService _apiService;
        private readonly SQLiteService _sqliteService;

        private AppDataResponse? _memoryCache;
        private string? _cachedLang;
        private static readonly TimeSpan AppDataCacheMaxAge = TimeSpan.FromHours(12);

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AppDataCacheService(ApiService apiService, SQLiteService sqliteService)
        {
            _apiService = apiService;
            _sqliteService = sqliteService;
        }

        public async Task<AppDataResponse> GetAsync(string lang = "vi", bool forceRefresh = false)
        {
            lang = string.IsNullOrWhiteSpace(lang) ? "vi" : lang.Trim().ToLowerInvariant();
            var cacheKey = $"appdata_{lang}";

            if (!forceRefresh && _memoryCache != null && _cachedLang == lang)
                return _memoryCache;

            // 1) Có mạng: ưu tiên gọi API
            if (Connectivity.Current.NetworkAccess == NetworkAccess.Internet)
            {
                try
                {
                    var apiData = await _apiService.GetAppDataAsync(lang);

                    if (apiData != null)
                    {
                        await _sqliteService.UpsertCacheAsync(new AppCacheEntry
                        {
                            CacheKey = cacheKey,
                            JsonData = JsonSerializer.Serialize(apiData, JsonOptions),
                            UpdatedAtUtc = DateTime.UtcNow
                        });

                        _memoryCache = apiData;
                        _cachedLang = lang;
                        return apiData;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AppDataCacheService] API error: {ex.Message}");
                }
            }

            // 2) Fallback SQLite
            try
            {
                var local = await _sqliteService.GetCacheIfFreshAsync(cacheKey, AppDataCacheMaxAge);

                if (local != null && !string.IsNullOrWhiteSpace(local.JsonData))
                {
                    var cachedData = JsonSerializer.Deserialize<AppDataResponse>(local.JsonData, JsonOptions);
                    if (cachedData != null)
                    {
                        _memoryCache = cachedData;
                        _cachedLang = lang;
                        return cachedData;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppDataCacheService] SQLite error: {ex.Message}");
            }

            // 3) Không có gì thì trả rỗng
            _memoryCache = new AppDataResponse();
            _cachedLang = lang;
            return _memoryCache;
        }

        public async Task RefreshAsync(string lang = "vi")
        {
            await GetAsync(lang, forceRefresh: true);
        }

        public async Task ClearAsync()
        {
            _memoryCache = null;
            _cachedLang = null;
            await _sqliteService.ClearAllCacheAsync();
        }

        public Task CleanupExpiredCacheAsync()
        {
            return _sqliteService.DeleteExpiredCacheAsync(AppDataCacheMaxAge);
        }

        public void ClearMemory()
        {
            _memoryCache = null;
            _cachedLang = null;
        }
    }
}
