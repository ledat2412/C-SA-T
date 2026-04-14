using System.Text.Json;
using MauiApp1.Models;

namespace MauiApp1.Services
{
    public class AppDataCacheService
    {
        private readonly ApiService _apiService;
        private readonly SQLiteService _sqliteService;
        private readonly AudioCacheService _audioCacheService;

        private readonly Dictionary<string, AppDataResponse> _memoryCache =
            new(StringComparer.OrdinalIgnoreCase);
        private static readonly TimeSpan AppDataCacheMaxAge = TimeSpan.FromHours(12);

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public AppDataCacheService(
            ApiService apiService,
            SQLiteService sqliteService,
            AudioCacheService audioCacheService)
        {
            _apiService = apiService;
            _sqliteService = sqliteService;
            _audioCacheService = audioCacheService;
        }

        public async Task<AppDataResponse> GetAsync(string lang = "vi", bool forceRefresh = false)
        {
            lang = string.IsNullOrWhiteSpace(lang) ? "vi" : lang.Trim().ToLowerInvariant();
            var cacheKey = $"appdata_{lang}";

            if (!forceRefresh && _memoryCache.TryGetValue(cacheKey, out var memoryData))
                return memoryData;

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

                        _memoryCache[cacheKey] = apiData;
                        _ = PrefetchAudioInBackgroundAsync(apiData);
                        return apiData;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AppDataCacheService] API error: {ex.Message}");
                }
            }

            try
            {
                var local = await _sqliteService.GetCacheIfFreshAsync(cacheKey, AppDataCacheMaxAge);
                if (local != null && !string.IsNullOrWhiteSpace(local.JsonData))
                {
                    var cachedData = JsonSerializer.Deserialize<AppDataResponse>(local.JsonData, JsonOptions);
                    if (cachedData != null)
                    {
                        _memoryCache[cacheKey] = cachedData;
                        return cachedData;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppDataCacheService] Fresh SQLite error: {ex.Message}");
            }

            try
            {
                var local = await _sqliteService.GetCacheAsync(cacheKey);
                if (local != null && !string.IsNullOrWhiteSpace(local.JsonData))
                {
                    var cachedData = JsonSerializer.Deserialize<AppDataResponse>(local.JsonData, JsonOptions);
                    if (cachedData != null)
                    {
                        _memoryCache[cacheKey] = cachedData;
                        return cachedData;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppDataCacheService] Stale SQLite error: {ex.Message}");
            }

            try
            {
                var fallback = await _sqliteService.GetLatestCacheByPrefixAsync("appdata_");
                if (fallback != null && !string.IsNullOrWhiteSpace(fallback.JsonData))
                {
                    var cachedData = JsonSerializer.Deserialize<AppDataResponse>(fallback.JsonData, JsonOptions);
                    if (cachedData != null)
                        return cachedData;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppDataCacheService] Latest cached fallback error: {ex.Message}");
            }

            var empty = new AppDataResponse();
            _memoryCache[cacheKey] = empty;
            return empty;
        }

        public async Task RefreshAsync(string lang = "vi")
        {
            await GetAsync(lang, forceRefresh: true);
        }

        public async Task ClearAsync()
        {
            _memoryCache.Clear();
            await _sqliteService.ClearAllCacheAsync();
        }

        public Task CleanupExpiredCacheAsync()
        {
            return _sqliteService.DeleteExpiredCacheAsync(AppDataCacheMaxAge);
        }

        public void ClearMemory()
        {
            _memoryCache.Clear();
        }

        private async Task PrefetchAudioInBackgroundAsync(AppDataResponse appData)
        {
            try
            {
                await _audioCacheService.PrefetchForAppDataAsync(appData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AppDataCacheService] Audio prefetch error: {ex.Message}");
            }
        }
    }
}
