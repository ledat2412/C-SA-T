using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;

namespace MauiApp1;

public class OsmMapPage : ContentPage
{
    private readonly WebView _web;
    private readonly Label _status;

    public OsmMapPage()
    {
        Title = "OpenStreetMap (Leaflet)";

        _status = new Label { Text = "Chưa lấy vị trí", FontSize = 14 };

        var btnLocate = new Button { Text = "📍 Lấy GPS & đưa lên bản đồ" };
        btnLocate.Clicked += async (_, __) => await LocateAndUpdateAsync();

        var btnHcm = new Button { Text = "🏙️ Về HCM (demo)" };
        btnHcm.Clicked += async (_, __) => await SetMarkerAsync(10.7769, 106.7009, "TP.HCM");

        _web = new WebView
        {
            Source = new HtmlWebViewSource { Html = BuildLeafletHtml() },
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        Content = new VerticalStackLayout
        {
            Padding = 12,
            Spacing = 10,
            Children = { _status, btnLocate, btnHcm, _web }
        };
    }

    private static string BuildLeafletHtml()
    {
        // Leaflet từ CDN (không cần key). Cần mạng để tải tile OSM + leaflet.
        return @"
<!doctype html>
<html>
<head>
  <meta charset='utf-8' />
  <meta name='viewport' content='width=device-width, initial-scale=1.0' />
  <link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css'
        integrity='sha256-p4NxAoJBhIIN+hmNHrzRCf9tD/miZyoHS5obTRR9BMY=' crossorigin=''/>
  <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'
        integrity='sha256-20nQCchB9co0qIjJZRGuk2/Z9VM+kNiyxNV1lvTlZBo=' crossorigin=''></script>
  <style>
    html, body { height:100%; margin:0; }
    #map { height:100vh; width:100%; }
  </style>
</head>
<body>
<div id='map'></div>

<script>
  // Default: TP.HCM
  var map = L.map('map').setView([10.7769, 106.7009], 13);

  L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; OpenStreetMap contributors'
  }).addTo(map);

  var marker = null;

  function setMarker(lat, lon, label) {
    if (marker) {
      marker.setLatLng([lat, lon]);
      if (label) marker.bindPopup(label);
    } else {
      marker = L.marker([lat, lon]).addTo(map);
      if (label) marker.bindPopup(label).openPopup();
    }
    map.setView([lat, lon], 16);
    return true;
  }

  function panTo(lat, lon, zoom) {
    map.setView([lat, lon], zoom || map.getZoom());
    return true;
  }
</script>
</body>
</html>";
    }

    private async Task LocateAndUpdateAsync()
    {
        try
        {
            _status.Text = "Đang xin quyền vị trí...";
            var permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (permission != PermissionStatus.Granted)
            {
                _status.Text = "❌ Không có quyền vị trí.";
                return;
            }

            _status.Text = "Đang lấy GPS...";
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var loc = await Geolocation.GetLocationAsync(request);

            if (loc is null)
            {
                _status.Text = "⚠️ Không lấy được vị trí (null).";
                return;
            }

            _status.Text = $"✅ {loc.Latitude:F6}, {loc.Longitude:F6} (±{loc.Accuracy}m)";
            await SetMarkerAsync(loc.Latitude, loc.Longitude, "Vị trí hiện tại");
        }
        catch (FeatureNotEnabledException)
        {
            _status.Text = "❌ GPS đang tắt. Bật Location trong Settings.";
        }
        catch (PermissionException)
        {
            _status.Text = "❌ Bạn chưa cấp quyền vị trí.";
        }
        catch (Exception ex)
        {
            _status.Text = "❌ Lỗi: " + ex.Message;
        }
    }

    private async Task SetMarkerAsync(double lat, double lon, string label)
    {
        // Ensure dot decimal for JS
        var latStr = lat.ToString("F6", CultureInfo.InvariantCulture);
        var lonStr = lon.ToString("F6", CultureInfo.InvariantCulture);
        label = label.Replace("'", "\\'");

        // Gọi function JS trong trang
        await _web.EvaluateJavaScriptAsync($"setMarker({latStr}, {lonStr}, '{label}');");
    }
}