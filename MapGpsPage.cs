using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Permissions = Microsoft.Maui.ApplicationModel.Permissions;
using PermissionStatus = Microsoft.Maui.ApplicationModel.PermissionStatus;

using Microsoft.Maui.Maps;
using Microsoft.Maui.Controls.Maps;

// ✅ FIX CS0104: alias Map control để không đụng Microsoft.Maui.ApplicationModel.Map
using MapControl = Microsoft.Maui.Controls.Maps.Map;

namespace MauiApp1;

public class MapGpsPage : ContentPage
{
    private readonly MapControl _map;
    private readonly Label _status;
    private readonly Label _info;

    public MapGpsPage()
    {
        Title = "Map + GPS";

        _status = new Label { Text = "Trạng thái: chưa lấy vị trí", FontSize = 14 };
        _info = new Label { Text = "-", FontSize = 14 };

        // mặc định center HCM
        var defaultCenter = new Location(10.7769, 106.7009);

        _map = new MapControl(
            MapSpan.FromCenterAndRadius(defaultCenter, Distance.FromKilometers(2)))
        {
            IsShowingUser = false
        };

        var btnLocate = new Button { Text = "📍 Lấy vị trí & zoom" };
        btnLocate.Clicked += async (_, __) => await LocateAndShowAsync();

        var btnOpenExternal = new Button { Text = "🗺️ Mở Google Maps (ngoài app)" };
        btnOpenExternal.Clicked += async (_, __) => await OpenExternalMapsAsync();

        Content = new VerticalStackLayout
        {
            Padding = 12,
            Spacing = 10,
            Children =
            {
                _status,
                btnLocate,
                btnOpenExternal,
                _info,
                _map
            }
        };
    }

    private async Task LocateAndShowAsync()
    {
        try
        {
            _status.Text = "Trạng thái: xin quyền vị trí...";

            var permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (permission != PermissionStatus.Granted)
            {
                _status.Text = "❌ Trạng thái: không có quyền vị trí";
                return;
            }

            _status.Text = "Trạng thái: đang lấy GPS...";

            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var loc = await Geolocation.GetLocationAsync(request);

            if (loc is null)
            {
                _status.Text = "⚠️ Trạng thái: không lấy được vị trí (null)";
                return;
            }

            var center = new Location(loc.Latitude, loc.Longitude);

            // pin
            _map.Pins.Clear();
            _map.Pins.Add(new Pin
            {
                Label = "Bạn đang ở đây",
                Location = center,
                Type = PinType.Place
            });

            // zoom
            _map.IsShowingUser = true;
            _map.MoveToRegion(MapSpan.FromCenterAndRadius(center, Distance.FromMeters(400)));

            _status.Text = "✅ Trạng thái: lấy GPS thành công";

            var sb = new StringBuilder();
            sb.AppendLine($"Lat: {loc.Latitude:F6}");
            sb.AppendLine($"Lon: {loc.Longitude:F6}");
            sb.AppendLine($"Accuracy: {loc.Accuracy} m");
            if (loc.Altitude != null) sb.AppendLine($"Altitude: {loc.Altitude} m");
            if (loc.Speed != null) sb.AppendLine($"Speed: {loc.Speed} m/s");
            sb.AppendLine($"Time: {loc.Timestamp.LocalDateTime}");
            _info.Text = sb.ToString();
        }
        catch (FeatureNotEnabledException)
        {
            _status.Text = "❌ Trạng thái: GPS đang tắt (bật Location trong Settings)";
        }
        catch (PermissionException)
        {
            _status.Text = "❌ Trạng thái: chưa được cấp quyền vị trí";
        }
        catch (Exception ex)
        {
            _status.Text = "❌ Lỗi: " + ex.Message;
        }
    }

    private async Task OpenExternalMapsAsync()
    {
        try
        {
            // mở app bản đồ ngoài (ApplicationModel.Map)
            var permission = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (permission != PermissionStatus.Granted)
                return;

            var loc = await Geolocation.GetLastKnownLocationAsync()
                      ?? await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Medium));

            if (loc is null) return;

            await Microsoft.Maui.ApplicationModel.Map.OpenAsync(
                loc.Latitude,
                loc.Longitude,
                new MapLaunchOptions { Name = "Vị trí hiện tại" });
        }
        catch
        {
            // bỏ qua lỗi mở maps ngoài
        }
    }
}