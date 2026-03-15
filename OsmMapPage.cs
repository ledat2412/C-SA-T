using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Maps;
using Permissions = Microsoft.Maui.ApplicationModel.Permissions;
using PermissionStatus = Microsoft.Maui.ApplicationModel.PermissionStatus;

namespace MauiApp1;

public partial class OsmMapPage : ContentPage
{
    private readonly Microsoft.Maui.Controls.Maps.Map _map;
    private readonly Label _status;

    public OsmMapPage()
    {
        Title = "Google Map";

        var defaultCenter = new Location(10.7769, 106.7009); // TP.HCM

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
        btnHcm.Clicked += (_, __) =>
        {
            ShowLocation(defaultCenter, "TP.HCM", "Demo vị trí trung tâm TP.HCM");
        };

        _map = new Microsoft.Maui.Controls.Maps.Map(
            MapSpan.FromCenterAndRadius(defaultCenter, Distance.FromKilometers(2)))
        {
            IsShowingUser = false,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill
        };

        _map.Pins.Add(new Pin
        {
            Label = "TP.HCM",
            Address = "Demo",
            Type = PinType.Place,
            Location = defaultCenter
        });

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

        Grid.SetRow(_status, 0);
        Grid.SetRow(btnLocate, 1);
        Grid.SetRow(btnHcm, 2);
        Grid.SetRow(_map, 3);

        grid.Children.Add(_status);
        grid.Children.Add(btnLocate);
        grid.Children.Add(btnHcm);
        grid.Children.Add(_map);

        Content = grid;
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

            var location = await Geolocation.Default.GetLastKnownLocationAsync();
            location ??= await Geolocation.Default.GetLocationAsync(
                new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10)));

            if (location is null)
            {
                _status.Text = "⚠️ Không lấy được vị trí hiện tại.";
                return;
            }

            var myLocation = new Location(location.Latitude, location.Longitude);
            var accuracy = location.Accuracy?.ToString("F0") ?? "?";

            ShowLocation(
                myLocation,
                "Vị trí hiện tại",
                $"✅ {location.Latitude:F6}, {location.Longitude:F6} (±{accuracy}m)");
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

    private void ShowLocation(Location location, string pinLabel, string statusText)
    {
        _map.Pins.Clear();
        _map.Pins.Add(new Pin
        {
            Label = pinLabel,
            Type = PinType.Place,
            Location = location
        });

        _map.IsShowingUser = pinLabel == "Vị trí hiện tại";
        _map.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromMeters(400)));
        _status.Text = statusText;
    }
}