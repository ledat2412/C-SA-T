using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace MauiApp1.Views.Maps;

public partial class PoiMapPage
{
    private async Task ShowCurrentLocationMarkerAsync(bool centerOnUser)
    {
        try
        {
            var permission = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (permission != PermissionStatus.Granted)
            {
                permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (permission != PermissionStatus.Granted)
                {
                    System.Diagnostics.Debug.WriteLine("[PoiMapPage] Location permission not granted.");
                    return;
                }
            }

            var location = await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(8)));

            location ??= await Geolocation.Default.GetLastKnownLocationAsync();
            if (location is null)
            {
                System.Diagnostics.Debug.WriteLine("[PoiMapPage] Could not resolve current location.");
                return;
            }

            var pinLocation = new Location(location.Latitude, location.Longitude);
            UpdateUserLocationPin(pinLocation, centerOnUser);
            _geofenceEngine.ClearDebugLocation();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PoiMapPage] ShowCurrentLocationMarkerAsync error: {ex.Message}");
        }
    }

    private void UpdateUserLocationPin(Location pinLocation, bool centerOnUser)
    {
        var shouldRecreatePin = _userLocationPin is null;

        if (_userLocationPin is not null)
        {
            _map.Pins.Remove(_userLocationPin);
        }

        _userLocationPin ??= new UserLocationPin
        {
            Label = _loc.Get("my_location"),
            Type = PinType.SavedPin
        };

        _userLocationPin.Address = $"{pinLocation.Latitude:F6}, {pinLocation.Longitude:F6}";
        _userLocationPin.Location = pinLocation;
        _map.Pins.Add(_userLocationPin);

        System.Diagnostics.Debug.WriteLine(
            $"[PoiMapPage] User location pin {(shouldRecreatePin ? "created" : "refreshed")} at {pinLocation.Latitude:F6}, {pinLocation.Longitude:F6}");

        if (centerOnUser)
            _map.MoveToRegion(MapSpan.FromCenterAndRadius(pinLocation, Distance.FromMeters(350)));
    }

    private async Task InitializeGeofenceAsync()
    {
        try
        {
            var gianHangs = await _gianHangService.GetAllAsync(_selectedLanguageCode);
            await _geofenceEngine.UpdateTargetsAsync(gianHangs, radiusMeters: 10);

            if (!_isLiveLocationSubscribed)
            {
                _geofenceEngine.EnteredGeofence += OnEnteredGeofence;
                _geofenceEngine.LocationUpdated += OnLiveLocationUpdated;
                _isLiveLocationSubscribed = true;
            }

            _geofenceEngine.AutoPlayAudioWhenEntered = true;
            _geofenceEngine.PollInterval = TimeSpan.FromSeconds(3);
            await _geofenceEngine.EvaluateNowAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PoiMapPage] InitializeGeofenceAsync error: {ex.Message}");
        }
    }

    private void OnEnteredGeofence(object? sender, GeofenceTriggeredEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine(
            $"[Geofence] Entered '{e.Target.Name}' at {e.DistanceMeters:F1}m (radius {e.Target.RadiusMeters:F0}m)");
    }

    private void OnLiveLocationUpdated(object? sender, LocationUpdatedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            UpdateUserLocationPin(e.Location, centerOnUser: false);

            if (_allPois.Count == 0)
                return;

            ApplySmartSearch(
                _searchEntry.Text,
                revealResults: false,
                preserveSelectedPoi: true);
        });
    }

    private async Task DiagnosePoiImagesAsync()
    {
        try
        {
            if (_pois.Count == 0)
                return;

            if (!HasInternet())
            {
                System.Diagnostics.Debug.WriteLine("[ImageProbe] Skip probe because device is offline.");
                return;
            }

            var failed = new List<string>();

            // Chỉ probe URL từ backend/API; ảnh local resource MAUI không probe qua HTTP.
            foreach (var poi in _pois.Take(20))
            {
                var source = NormalizeImagePath(poi.ImagePath);
                if (!source.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !source.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var error = await ProbeImageUrlAsync(source, poi.Title);
                if (error is not null)
                {
                    failed.Add(error);
                }
            }

            if (failed.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[ImageProbe] Failed image count: {failed.Count}");
                foreach (var item in failed.Take(5))
                {
                    System.Diagnostics.Debug.WriteLine($"[ImageProbe][ERROR] {item}");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[ImageProbe] All probed POI images are reachable.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ImageProbe] Diagnose error: {ex.Message}");
        }
    }

    private static async Task<string?> ProbeImageUrlAsync(string imageUrl, string poiTitle)
    {
        try
        {
            _imageProbeHttpClient ??= CreateImageProbeHttpClient();

            using var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
            using var response = await _imageProbeHttpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return $"'{poiTitle}' -> {imageUrl} | HTTP {(int)response.StatusCode} {response.StatusCode}";
            }

            return null;
        }
        catch (Exception ex)
        {
            return $"'{poiTitle}' -> {imageUrl} | EX: {ex.Message}";
        }
    }

    private static HttpClient CreateImageProbeHttpClient()
    {
        var handler = new HttpClientHandler();

#if DEBUG
        handler.ServerCertificateCustomValidationCallback =
            (message, cert, chain, errors) => true;
#endif

        var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

        global::MauiApp1.Utils.BackendUrlResolver.ConfigureHttpClient(httpClient);
        return httpClient;
    }

}


