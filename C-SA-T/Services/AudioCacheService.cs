using System.Collections.Concurrent;
using System.Security.Cryptography;
using MauiApp1.Models;
using MauiApp1.Utils;

namespace MauiApp1.Services;

public sealed class AudioCacheService
{
    private static readonly TimeSpan FreshAudioCacheAge = TimeSpan.FromDays(7);
    private static readonly ConcurrentDictionary<string, byte[]> MemoryCache =
        new(StringComparer.OrdinalIgnoreCase);

    private static HttpClient? _audioHttpClient;

    private readonly SQLiteService _sqliteService;

    public AudioCacheService(SQLiteService sqliteService)
    {
        _sqliteService = sqliteService;
    }

    public async Task<byte[]?> GetAudioBytesAsync(string? audioUrl, CancellationToken cancellationToken = default)
    {
        var normalizedUrl = NormalizeAudioUrl(audioUrl);
        if (string.IsNullOrWhiteSpace(normalizedUrl))
            return null;

        if (MemoryCache.TryGetValue(normalizedUrl, out var memoryBytes) && memoryBytes.Length > 0)
            return memoryBytes;

        var cachedBytes = await TryReadCachedBytesAsync(normalizedUrl, allowStale: true).ConfigureAwait(false);
        if (cachedBytes is not null)
        {
            MemoryCache[normalizedUrl] = cachedBytes;
            return cachedBytes;
        }

        if (!HasInternet())
            return null;

        return await TryDownloadAndCacheAsync(normalizedUrl, cancellationToken).ConfigureAwait(false);
    }

    public async Task PrefetchForAppDataAsync(AppDataResponse? appData, CancellationToken cancellationToken = default)
    {
        if (appData is null || appData.GianHangs.Count == 0 || !HasInternet())
            return;

        var audioUrls = appData.GianHangs
            .Select(x => x.AudioFullUrl)
            .Select(NormalizeAudioUrl)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Cast<string>()
            .ToList();

        foreach (var audioUrl in audioUrls)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (MemoryCache.ContainsKey(audioUrl))
                    continue;

                var cacheKey = BuildAudioCacheKey(audioUrl);
                var freshEntry = await _sqliteService
                    .GetCacheIfFreshAsync(cacheKey, FreshAudioCacheAge)
                    .ConfigureAwait(false);

                if (freshEntry is not null && !string.IsNullOrWhiteSpace(freshEntry.JsonData))
                {
                    try
                    {
                        MemoryCache[audioUrl] = Convert.FromBase64String(freshEntry.JsonData);
                        continue;
                    }
                    catch (FormatException)
                    {
                        // Fall through and refresh this audio from the network.
                    }
                }

                await TryDownloadAndCacheAsync(audioUrl, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AudioCache] Prefetch failed for {audioUrl}: {ex.Message}");
            }
        }
    }

    private async Task<byte[]?> TryDownloadAndCacheAsync(string audioUrl, CancellationToken cancellationToken)
    {
        try
        {
            _audioHttpClient ??= CreateAudioHttpClient();

            using var response = await _audioHttpClient
                .GetAsync(audioUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[AudioCache] Network fetch failed {audioUrl} -> {(int)response.StatusCode} {response.StatusCode}");
                return null;
            }

            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            if (bytes.Length == 0)
                return null;

            MemoryCache[audioUrl] = bytes;

            await _sqliteService.UpsertCacheAsync(new AppCacheEntry
            {
                CacheKey = BuildAudioCacheKey(audioUrl),
                JsonData = Convert.ToBase64String(bytes),
                UpdatedAtUtc = DateTime.UtcNow
            }).ConfigureAwait(false);

            return bytes;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioCache] Download failed for {audioUrl}: {ex.Message}");
            return null;
        }
    }

    private async Task<byte[]?> TryReadCachedBytesAsync(string audioUrl, bool allowStale)
    {
        try
        {
            var cacheKey = BuildAudioCacheKey(audioUrl);
            var entry = allowStale
                ? await _sqliteService.GetCacheAsync(cacheKey).ConfigureAwait(false)
                : await _sqliteService.GetCacheIfFreshAsync(cacheKey, FreshAudioCacheAge).ConfigureAwait(false);

            if (entry is null || string.IsNullOrWhiteSpace(entry.JsonData))
                return null;

            return Convert.FromBase64String(entry.JsonData);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioCache] Cache read failed for {audioUrl}: {ex.Message}");
            return null;
        }
    }

    private static string NormalizeAudioUrl(string? audioUrl)
    {
        if (string.IsNullOrWhiteSpace(audioUrl))
            return string.Empty;

        var normalized = audioUrl.Trim();
        if (normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return normalized;
        }

        return BackendUrlResolver.BuildUrl(normalized);
    }

    private static bool HasInternet()
    {
        return Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
    }

    private static string BuildAudioCacheKey(string audioUrl)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(audioUrl));
        return $"aud_bin_{Convert.ToHexString(bytes)}";
    }

    private static HttpClient CreateAudioHttpClient()
    {
        var handler = new HttpClientHandler();

#if DEBUG
        handler.ServerCertificateCustomValidationCallback =
            (_, _, _, _) => true;
#endif

        var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(12)
        };

        BackendUrlResolver.ConfigureHttpClient(httpClient);
        return httpClient;
    }
}
