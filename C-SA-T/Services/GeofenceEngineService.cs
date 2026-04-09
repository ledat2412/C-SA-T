using MauiApp1.Models;
using Plugin.Maui.Audio;

namespace MauiApp1.Services;

public sealed class GeofenceEngineService : IAsyncDisposable
{
    private readonly IAudioManager _audioManager;
    private readonly SemaphoreSlim _sync = new(1, 1);

    private readonly Dictionary<int, GeofenceTarget> _targets = new();
    private readonly HashSet<int> _insideTargetIds = new();

    private CancellationTokenSource? _loopCts;
    private Task? _loopTask;
    private IAudioPlayer? _currentPlayer;
    private MemoryStream? _currentAudioStream;
    private Location? _debugLocationOverride;

    public event EventHandler<GeofenceTriggeredEventArgs>? EnteredGeofence;

    public bool AutoPlayAudioWhenEntered { get; set; } = true;
    public double RadiusMeters { get; set; } = 10d;
    public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(3);

    public GeofenceEngineService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public async Task UpdateTargetsAsync(IEnumerable<GianHang> gianHangs, double? radiusMeters = null)
    {
        await _sync.WaitAsync();
        try
        {
            _targets.Clear();
            _insideTargetIds.Clear();

            foreach (var gianHang in gianHangs)
            {
                if (gianHang.Lat is null || gianHang.Lon is null)
                    continue;

                _targets[gianHang.IdGianHang] = new GeofenceTarget(
                    gianHang.IdGianHang,
                    string.IsNullOrWhiteSpace(gianHang.Ten) ? $"POI {gianHang.IdGianHang}" : gianHang.Ten,
                    gianHang.Lat.Value,
                    gianHang.Lon.Value,
                    gianHang.AudioFullUrl,
                    radiusMeters ?? RadiusMeters);
            }
        }
        finally
        {
            _sync.Release();
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_loopTask is { IsCompleted: false })
            return;

        var permission = await EnsurePermissionAsync();
        if (!permission)
            return;

        _loopCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _loopTask = Task.Run(() => RunLoopAsync(_loopCts.Token), _loopCts.Token);
    }

    public async Task StopAsync()
    {
        if (_loopCts is null)
            return;

        try
        {
            _loopCts.Cancel();
            if (_loopTask is not null)
                await _loopTask;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _loopCts.Dispose();
            _loopCts = null;
            _loopTask = null;
            StopCurrentAudio();
        }
    }

    public void SetDebugLocation(double latitude, double longitude)
    {
        _debugLocationOverride = new Location(latitude, longitude);
    }

    public void ClearDebugLocation()
    {
        _debugLocationOverride = null;
    }

    public async Task EvaluateNowAsync(CancellationToken cancellationToken = default)
    {
        await EvaluateCurrentLocationAsync(cancellationToken);
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await EvaluateCurrentLocationAsync(ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GeofenceEngine] Loop error: {ex.Message}");
            }

            try
            {
                await Task.Delay(PollInterval, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task EvaluateCurrentLocationAsync(CancellationToken ct)
    {
        var location = _debugLocationOverride;
        if (location is null)
        {
            location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(6)),
                ct);

            location ??= await Geolocation.Default.GetLastKnownLocationAsync();
        }

        if (location is null)
            return;

        List<GeofenceTriggeredEventArgs> triggers = [];

        await _sync.WaitAsync(ct);
        try
        {
            foreach (var target in _targets.Values)
            {
                var distance = Location.CalculateDistance(
                    location.Latitude,
                    location.Longitude,
                    target.Latitude,
                    target.Longitude,
                    DistanceUnits.Kilometers) * 1000d;

                var isInside = distance <= target.RadiusMeters;

                if (isInside)
                {
                    if (_insideTargetIds.Add(target.Id))
                    {
                        triggers.Add(new GeofenceTriggeredEventArgs(target, distance));
                    }
                }
                else
                {
                    _insideTargetIds.Remove(target.Id);
                }
            }
        }
        finally
        {
            _sync.Release();
        }

        foreach (var trigger in triggers)
        {
            EnteredGeofence?.Invoke(this, trigger);
            if (AutoPlayAudioWhenEntered)
                await AutoPlayAudioAsync(trigger.Target.AudioUrl, ct);
        }
    }

    private static async Task<bool> EnsurePermissionAsync()
    {
        var permission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (permission == PermissionStatus.Granted)
            return true;

        permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        return permission == PermissionStatus.Granted;
    }

    private async Task AutoPlayAudioAsync(string? audioUrl, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(audioUrl))
            return;

        try
        {
            StopCurrentAudio();

            using var client = CreateAudioHttpClient();
            var bytes = await client.GetByteArrayAsync(audioUrl, ct);

            _currentAudioStream = new MemoryStream(bytes);
            _currentPlayer = _audioManager.CreatePlayer(_currentAudioStream);
            _currentPlayer.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[GeofenceEngine] Auto-play error: {ex.Message}");
        }
    }

    private static HttpClient CreateAudioHttpClient()
    {
        var handler = new HttpClientHandler();
#if DEBUG
        handler.ServerCertificateCustomValidationCallback =
            (_, _, _, _) => true;
#endif
        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
    }

    private void StopCurrentAudio()
    {
        try
        {
            _currentPlayer?.Stop();
            _currentPlayer?.Dispose();
            _currentPlayer = null;

            _currentAudioStream?.Dispose();
            _currentAudioStream = null;
        }
        catch
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        _sync.Dispose();
    }
}

public sealed record GeofenceTarget(
    int Id,
    string Name,
    double Latitude,
    double Longitude,
    string? AudioUrl,
    double RadiusMeters);

public sealed record GeofenceTriggeredEventArgs(GeofenceTarget Target, double DistanceMeters);
