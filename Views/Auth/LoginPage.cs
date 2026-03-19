using MauiApp1.Services;
using MauiApp1.Views.Maps;
using Microsoft.Maui.Controls;
using System;

namespace MauiApp1.Views.Auth;

public class LoginPage : ContentPage
{
    private readonly MySqlService _mySqlService;

    readonly Entry _usernameEntry;
    readonly Entry _passwordEntry;
    readonly Label _statusLabel;

    public LoginPage(MySqlService mySqlService)
    {
        _mySqlService = mySqlService;

        Title = "Login";

        var title = new Label
        {
            Text = "Đăng nhập",
            FontSize = 28,
            HorizontalOptions = LayoutOptions.Center,
            FontAttributes = FontAttributes.Bold
        };

        _usernameEntry = new Entry
        {
            Placeholder = "Nhập username"
        };

        _passwordEntry = new Entry
        {
            Placeholder = "Nhập password",
            IsPassword = true
        };

        var loginButton = new Button
        {
            Text = "Đăng nhập"
        };

        var registerButton = new Button
        {
            Text = "Đăng ký"
        };

        _statusLabel = new Label
        {
            Text = "",
            TextColor = Colors.Red,
            HorizontalOptions = LayoutOptions.Center
        };

        loginButton.Clicked += OnLoginClicked;
        registerButton.Clicked += OnRegisterClicked;

        Content = new VerticalStackLayout
        {
            Padding = 24,
            Spacing = 16,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                title,
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
        try
        {
            string username = _usernameEntry.Text?.Trim() ?? "";
            string password = _passwordEntry.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _statusLabel.Text = "Vui lòng nhập đầy đủ tài khoản và mật khẩu";
                return;
            }

            _statusLabel.Text = "Đang kiểm tra đăng nhập...";

            bool isValid = await _mySqlService.LoginAsync(username, password);

            if (isValid)
            {
                _statusLabel.Text = "Đăng nhập thành công";
                await Navigation.PushAsync(new OsmMapPage());
            }
            else
            {
                _statusLabel.Text = "Sai tài khoản hoặc mật khẩu";
                await DisplayAlert("Thông báo", "Sai tài khoản hoặc mật khẩu", "OK");
            }
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Lỗi kết nối database";
            await DisplayAlert("Lỗi", ex.ToString(), "OK");
        }
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        await Navigation.PushAsync(new RegisterPage(_mySqlService));
    }
}