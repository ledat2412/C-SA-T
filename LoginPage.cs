using System;
using Microsoft.Maui.Controls;

namespace MauiApp1;

public class LoginPage : ContentPage
{
    readonly Entry _usernameEntry;
    readonly Entry _passwordEntry;
    readonly Label _statusLabel;

    public LoginPage()
    {
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

        _statusLabel = new Label
        {
            Text = "",
            TextColor = Colors.Red,
            HorizontalOptions = LayoutOptions.Center
        };

        loginButton.Clicked += OnLoginClicked;

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
                _statusLabel
            }
        };
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        string username = _usernameEntry.Text?.Trim() ?? "";
        string password = _passwordEntry.Text?.Trim() ?? "";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            _statusLabel.Text = "Vui lòng nhập đầy đủ tài khoản và mật khẩu";
            return;
        }

        // Tài khoản demo
        if (username == "admin" && password == "123")
        {
            await Navigation.PushAsync(new OsmMapPage());
        }
        else
        {
            _statusLabel.Text = "Sai tài khoản hoặc mật khẩu";
        }
    }
}