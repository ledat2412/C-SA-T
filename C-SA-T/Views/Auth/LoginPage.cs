using System;
using Microsoft.Maui.Controls;
using MauiApp1.Services;
using MauiApp1.Views.Maps;
using Microsoft.Extensions.DependencyInjection;

namespace MauiApp1.Views.Auth;

public class LoginPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IServiceProvider _serviceProvider;

    private Entry _usernameEntry;
    private Entry _passwordEntry;
    private Label _statusLabel;
    private Button _loginButton;
    private bool _isLoggingIn;

    public LoginPage(ApiService apiService, IServiceProvider serviceProvider)
    {
        _apiService = apiService;
        _serviceProvider = serviceProvider;

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
            await DisplayAlertAsync("Thông báo", "Chưa làm chức năng đăng ký", "OK");
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
        if (_isLoggingIn)
            return;

        string username = _usernameEntry.Text?.Trim() ?? "";
        string password = _passwordEntry.Text?.Trim() ?? "";

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
            var result = await _apiService.LoginAsync(username, password);

            if (result.Success)
            {
                _statusLabel.Text = "Đăng nhập thành công";
                var poiMapPage = _serviceProvider.GetRequiredService<PoiMapPage>();
                await Navigation.PushAsync(poiMapPage);
            }
            else
            {
                _statusLabel.Text = string.IsNullOrWhiteSpace(result.Message)
                    ? "Sai tài khoản hoặc mật khẩu"
                    : result.Message;
            }
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Lỗi kết nối API";
            await DisplayAlertAsync("Error", ex.Message, "OK");
        }
        finally
        {
            _isLoggingIn = false;
            _loginButton.IsEnabled = true;
        }
    }
}
