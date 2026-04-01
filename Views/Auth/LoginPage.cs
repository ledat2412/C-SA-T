using System;
using Microsoft.Maui.Controls;
using MauiApp1.Services;
using MauiApp1.Views.Maps;
using Microsoft.Extensions.DependencyInjection;

namespace MauiApp1.Views.Auth;

public class LoginPage : ContentPage
{
    private readonly TaiKhoanService _taiKhoanService;
<<<<<<< HEAD
    private readonly IServiceProvider _serviceProvider;
=======
    private readonly PoiMapPage _poiMapPage;
>>>>>>> 2e8f476d66d60fa26b4337d437e259a134102afb

    private Entry _usernameEntry;
    private Entry _passwordEntry;
    private Label _statusLabel;
    private Button _loginButton;
    private bool _isLoggingIn;

<<<<<<< HEAD
    public LoginPage(TaiKhoanService taiKhoanService, IServiceProvider serviceProvider)
    {
        _taiKhoanService = taiKhoanService;
        _serviceProvider = serviceProvider;
=======
    public LoginPage(TaiKhoanService taiKhoanService, PoiMapPage poiMapPage)
    {
        _taiKhoanService = taiKhoanService;
        _poiMapPage = poiMapPage;
>>>>>>> 2e8f476d66d60fa26b4337d437e259a134102afb

        Title = "Đăng nhập";

        _usernameEntry = new Entry
        {
            Placeholder = "Tên đăng nhập hoặc email"
        };

        _passwordEntry = new Entry
        {
            Placeholder = "Mật khẩu",
            IsPassword = true
        };

        _loginButton = new Button
        {
            Text = "Đăng nhập",
            BackgroundColor = Colors.Blue,
            TextColor = Colors.White
        };
        _loginButton.Clicked += OnLoginClicked;

        var registerButton = new Button
        {
            Text = "Đăng ký"
        };
        registerButton.Clicked += async (_, __) =>
        {
            await DisplayAlert("Thông báo", "Chưa làm chức năng đăng ký", "OK");
        };

        _statusLabel = new Label
        {
            Text = "",
            TextColor = Colors.Red,
            HorizontalOptions = LayoutOptions.Center
        };

        Content = new VerticalStackLayout
        {
            Padding = 30,
            Spacing = 15,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = "LOGIN",
                    FontSize = 24,
                    HorizontalOptions = LayoutOptions.Center
                },
                _usernameEntry,
                _passwordEntry,
                _loginButton,
                registerButton,
                _statusLabel
            }
        };
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
<<<<<<< HEAD
        if (_isLoggingIn)
            return;

        string username = _usernameEntry.Text?.Trim() ?? "";
        string password = _passwordEntry.Text?.Trim() ?? "";

=======
        string username = _usernameEntry.Text?.Trim() ?? "";
        string password = _passwordEntry.Text?.Trim() ?? "";

>>>>>>> 2e8f476d66d60fa26b4337d437e259a134102afb
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _statusLabel.Text = "Vui lòng nhập đầy đủ thông tin";
            return;
        }

        _isLoggingIn = true;
        _loginButton.IsEnabled = false;
        _statusLabel.Text = "Đang đăng nhập...";

        try
        {
            bool isValid = await _taiKhoanService.LoginAsync(username, password);

            if (isValid)
            {
                _statusLabel.Text = "Đăng nhập thành công";
<<<<<<< HEAD
                var poiMapPage = _serviceProvider.GetRequiredService<PoiMapPage>();
                await Navigation.PushAsync(poiMapPage);
=======
                await Navigation.PushAsync(_poiMapPage);
>>>>>>> 2e8f476d66d60fa26b4337d437e259a134102afb
            }
            else
            {
                _statusLabel.Text = "Sai tài khoản hoặc mật khẩu";
            }
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Lỗi kết nối DB";
            await DisplayAlert("Error", ex.Message, "OK");
        }
        finally
        {
            _isLoggingIn = false;
            _loginButton.IsEnabled = true;
        }
    }
}