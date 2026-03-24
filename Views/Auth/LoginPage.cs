using System;
using Microsoft.Maui.Controls;
using MauiApp1.Services;
using MauiApp1.Views.Maps;

namespace MauiApp1.Views.Auth;

public class LoginPage : ContentPage
{
    private readonly TaiKhoanService _taiKhoanService;
    private readonly PoiMapPage _poiMapPage;

    private Entry _usernameEntry;
    private Entry _passwordEntry;
    private Label _statusLabel;

    public LoginPage(TaiKhoanService taiKhoanService, PoiMapPage poiMapPage)
    {
        _taiKhoanService = taiKhoanService;
        _poiMapPage = poiMapPage;

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

        var loginButton = new Button
        {
            Text = "Đăng nhập",
            BackgroundColor = Colors.Blue,
            TextColor = Colors.White
        };
        loginButton.Clicked += OnLoginClicked;

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
                loginButton,
                registerButton,
                _statusLabel
            }
        };
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        string username = _usernameEntry.Text?.Trim() ?? "";
        string password = _passwordEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _statusLabel.Text = "Vui lòng nhập đầy đủ thông tin";
            return;
        }

        _statusLabel.Text = "Đang đăng nhập...";

        try
        {
            bool isValid = await _taiKhoanService.LoginAsync(username, password);

            if (isValid)
            {
                _statusLabel.Text = "Đăng nhập thành công";
                await Navigation.PushAsync(_poiMapPage);
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
    }
}