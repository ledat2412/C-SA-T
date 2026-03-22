using System;
using Microsoft.Maui.Controls;
using MauiApp1.Services;
using MauiApp1.Views.Maps;

namespace MauiApp1.Views.Auth;

public class LoginPage : ContentPage
{
    private Entry _usernameEntry;
    private Entry _passwordEntry;
    private Label _statusLabel;

    private readonly MySqlService _db = new MySqlService();

    public LoginPage()
    {
        Title = "Đăng nhập";

        _usernameEntry = new Entry
        {
            Placeholder = "Tên đăng nhập"
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

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = _usernameEntry.Text?.Trim();
        string password = _passwordEntry.Text?.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _statusLabel.Text = "Vui lòng nhập đầy đủ thông tin";
            return;
        }

        _statusLabel.Text = "Đang đăng nhập...";

        try
        {
            bool isValid = await _db.LoginAsync(username, password);

            if (isValid)
            {
                _statusLabel.Text = "Đăng nhập thành công";

                // 👉 chuyển sang trang chính (map)
                await Navigation.PushAsync(new PoiMapPage());
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