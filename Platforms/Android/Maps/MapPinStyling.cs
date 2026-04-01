using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Maui.Maps.Handlers;
using MauiApp1.Models;
using System.Collections.Generic;

namespace MauiApp1.Platforms.Android.Maps;

internal static class MapPinStyling
{
    private static bool _isConfigured;
    private static readonly object _styledMapsLock = new();
    private static readonly HashSet<nint> _styledMapHandles = [];
    private static readonly object _iconCacheLock = new();
    private static readonly Dictionary<double, BitmapDescriptor> _iconCache = [];

    // Custom theme - Light style với màu sắc tối ưu
    private static readonly string CustomMapStyle = "[" +
        "{\"elementType\":\"geometry\",\"stylers\":[{\"color\":\"#f5f5f5\"}]}" +
        ",{\"elementType\":\"labels.icon\",\"stylers\":[{\"visibility\":\"off\"}]}" +
        ",{\"elementType\":\"labels.text.fill\",\"stylers\":[{\"color\":\"#616161\"}]}" +
        ",{\"elementType\":\"labels.text.stroke\",\"stylers\":[{\"color\":\"#f5f5f5\"}]}" +
        ",{\"featureType\":\"administrative\",\"elementType\":\"geometry.stroke\",\"stylers\":[{\"color\":\"#c9c9c9\"}]}" +
        ",{\"featureType\":\"administrative.land_parcel\",\"elementType\":\"labels.text.fill\",\"stylers\":[{\"color\":\"#bdbdbd\"}]}" +
        ",{\"featureType\":\"poi\",\"elementType\":\"geometry\",\"stylers\":[{\"color\":\"#eeeeee\"}]}" +
        ",{\"featureType\":\"poi\",\"elementType\":\"labels.text.fill\",\"stylers\":[{\"color\":\"#757575\"}]}" +
        ",{\"featureType\":\"poi.park\",\"elementType\":\"geometry\",\"stylers\":[{\"color\":\"#e5e5e5\"}]}" +
        ",{\"featureType\":\"poi.park\",\"elementType\":\"labels.text.fill\",\"stylers\":[{\"color\":\"#9e9e9e\"}]}" +
        ",{\"featureType\":\"road\",\"elementType\":\"geometry\",\"stylers\":[{\"color\":\"#ffffff\"}]}" +
        ",{\"featureType\":\"road.arterial\",\"elementType\":\"labels.text.fill\",\"stylers\":[{\"color\":\"#757575\"}]}" +
        ",{\"featureType\":\"road.highway\",\"elementType\":\"geometry\",\"stylers\":[{\"color\":\"#dadada\"}]}" +
        ",{\"featureType\":\"road.highway\",\"elementType\":\"labels.text.fill\",\"stylers\":[{\"color\":\"#616161\"}]}" +
        ",{\"featureType\":\"road.local\",\"elementType\":\"labels.text.fill\",\"stylers\":[{\"color\":\"#9e9e9e\"}]}" +
        ",{\"featureType\":\"transit.line\",\"elementType\":\"geometry\",\"stylers\":[{\"color\":\"#e5e5e5\"}]}" +
        ",{\"featureType\":\"transit.station\",\"elementType\":\"geometry\",\"stylers\":[{\"color\":\"#eeeeee\"}]}" +
        ",{\"featureType\":\"water\",\"elementType\":\"geometry\",\"stylers\":[{\"color\":\"#c9c9c9\"}]}" +
        ",{\"featureType\":\"water\",\"elementType\":\"labels.text.fill\",\"stylers\":[{\"color\":\"#9e9e9e\"}]}" +
        "]";

    public static void Configure()
    {
        if (_isConfigured)
            return;

        _isConfigured = true;

        MapHandler.Mapper.AppendToMapping("CustomMapTheme", (handler, _) =>
        {
            ApplyCustomTheme(handler);
        });

        MapPinHandler.Mapper.AppendToMapping("StyledPin", (handler, pin) =>
        {
            if (pin is not StyledPin styledPin || handler.PlatformView is null)
                return;

            var icon = GetOrCreateIcon(styledPin.Rating);
            handler.PlatformView.SetIcon(icon);
            handler.PlatformView.Anchor(0.5f, 1f);
        });
    }

    public static void ApplyThemeToMap(Microsoft.Maui.Controls.Maps.Map map)
    {
        if (map.Handler is not IMapHandler handler || handler.PlatformView is null)
            return;

        ApplyCustomTheme(handler);
    }

    private static void ApplyCustomTheme(IMapHandler handler)
    {
        if (handler.PlatformView is null)
            return;

        var mapHandle = handler.PlatformView.Handle;
        if (mapHandle != nint.Zero)
        {
            lock (_styledMapsLock)
            {
                if (!_styledMapHandles.Add(mapHandle))
                    return;
            }
        }

        handler.PlatformView.GetMapAsync(new MapReadyCallback(googleMap =>
        {
            var applied = googleMap.SetMapStyle(new MapStyleOptions(CustomMapStyle));
            System.Diagnostics.Debug.WriteLine($"Map theme applied: {applied}");
        }));
    }

    private static BitmapDescriptor GetOrCreateIcon(double rating)
    {
        var normalizedRating = Math.Round(rating, 1);

        lock (_iconCacheLock)
        {
            if (_iconCache.TryGetValue(normalizedRating, out var cachedIcon))
                return cachedIcon;

            var markerBitmap = CustomMarkerFactory.Create(global::Android.App.Application.Context, normalizedRating);
            var icon = BitmapDescriptorFactory.FromBitmap(markerBitmap);
            _iconCache[normalizedRating] = icon;
            return icon;
        }
    }

    private sealed class MapReadyCallback(Action<GoogleMap> onMapReady) : global::Java.Lang.Object, IOnMapReadyCallback
    {
        public void OnMapReady(GoogleMap googleMap) => onMapReady(googleMap);
    }
}
