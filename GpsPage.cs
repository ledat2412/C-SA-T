using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;

namespace MauiApp1;

public class GpsPage : ContentPage
{
    readonly Label _status = new() { Text = "GPS: chưa lấy", FontSize = 16 };
    readonly Label _data = new() { Text = "-", FontSize = 16 };

    public GpsPage()
    {
        Title = "GPS Demo";

        var btn = new Button { Text = "📍 Lấy vị trí hiện tại" };
        btn.Clicked += async (_, __) => await GetLocationAsync();

        Content = new VerticalStackLayout
        {
            Padding = 24,
            Spacing = 12,
            Children = { _status, btn, _data }
        };
    }

    async Task GetLocationAsync()
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

            // Ưu tiên GPS (High) và timeout
            var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request);

            if (location is null)
            {
                _status.Text = "⚠️ Không lấy được vị trí (null).";
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Latitude:  {location.Latitude}");
            sb.AppendLine($"Longitude: {location.Longitude}");
            sb.AppendLine($"Accuracy:  {location.Accuracy} m");
            if (location.Altitude != null) sb.AppendLine($"Altitude:  {location.Altitude} m");
            if (location.Speed != null) sb.AppendLine($"Speed:     {location.Speed} m/s");
            if (location.Course != null) sb.AppendLine($"Course:    {location.Course}°");
            sb.AppendLine($"Time:      {location.Timestamp.LocalDateTime}");

            _status.Text = "✅ Lấy GPS thành công";
            _data.Text = sb.ToString();
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
}