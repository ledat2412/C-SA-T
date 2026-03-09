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

        _status = new Label
        {
            Text = "Chưa lấy vị trí",
            FontSize = 14
        };

        var btnLocate = new Button
        {
            Text = "📍 Lấy GPS & đưa lên bản đồ"
        };
        btnLocate.Clicked += async (_, __) => await LocateAndUpdateAsync();

        var btnHcm = new Button
        {
            Text = "🏙️ Về HCM (demo)"
        };
        btnHcm.Clicked += async (_, __) => await SetMarkerAsync(10.7769, 106.7009, "TP.HCM");

        _web = new WebView
        {
            Source = new HtmlWebViewSource
            {
                Html = BuildLeafletHtml()
            },
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Fill
        };

        var grid = new Grid
        {
            Padding = 12,
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }
            }
        };

        grid.Add(_status);
        Grid.SetRow(_status, 0);

        grid.Add(btnLocate);
        Grid.SetRow(btnLocate, 1);

        grid.Add(btnHcm);
        Grid.SetRow(btnHcm, 2);

        grid.Add(_web);
        Grid.SetRow(_web, 3);

        Content = grid;
    }

    private static string BuildLeafletHtml()
    {
        return @"
<!doctype html>
<html>
<head>
  <meta charset='utf-8' />
  <meta name='viewport' content='width=device-width, initial-scale=1.0' />

  <link rel='stylesheet'
        href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />

  <script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>

  <style>
    html, body, #map {
      height: 100%;
      margin: 0;
      padding: 0;
    }
  </style>
</head>
<body>
  <div id='map'></div>

  <script>
    var map = L.map('map').setView([10.7769, 106.7009], 13);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);

    var marker = null;

    function setMarker(lat, lon, label) {
      if (marker) {
        marker.setLatLng([lat, lon]);
        if (label) {
          marker.bindPopup(label).openPopup();
        }
      } else {
        marker = L.marker([lat, lon]).addTo(map);
        if (label) {
          marker.bindPopup(label).openPopup();
        }
      }

      map.setView([lat, lon], 16);

      setTimeout(function () {
        map.invalidateSize();
      }, 200);

      return true;
    }

    function panTo(lat, lon, zoom) {
      map.setView([lat, lon], zoom || map.getZoom());

      setTimeout(function () {
        map.invalidateSize();
      }, 200);

      return true;
    }

    setTimeout(function () {
      map.invalidateSize();
    }, 300);
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

            _status.Text = $"✅ {loc.Latitude:F6}, {loc.Longitude:F6} (±{loc.Accuracy?.ToString("F0") ?? "?"}m)";
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
        string latStr = lat.ToString("F6", CultureInfo.InvariantCulture);
        string lonStr = lon.ToString("F6", CultureInfo.InvariantCulture);
        string safeLabel = (label ?? string.Empty).Replace("'", "\\'");

        await _web.EvaluateJavaScriptAsync($"setMarker({latStr}, {lonStr}, '{safeLabel}');");
    }
}