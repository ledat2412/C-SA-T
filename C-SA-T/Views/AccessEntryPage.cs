using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Shapes;
using MauiApp1.Services;

namespace MauiApp1.Views;

public class AccessEntryPage : ContentPage
{
    private readonly AccessFlowService _accessFlowService;
    private readonly LocalizationService _loc;
    private readonly Label _statusLabel;
    private readonly Label _subtitleLabel;
    private readonly ActivityIndicator _loading;
    private readonly VerticalStackLayout _optionsLayout;
    private bool _isChecking;

    public AccessEntryPage(AccessFlowService accessFlowService, LocalizationService localizationService)
    {
        _accessFlowService = accessFlowService;
        _loc = localizationService;

        NavigationPage.SetHasNavigationBar(this, false);
        BackgroundColor = Color.FromArgb("#FFF7F1");
        Title = string.Empty;

        _statusLabel = new Label
        {
            Text = "Dang kiem tra truy cap...",
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#111111")
        };

        _subtitleLabel = new Label
        {
            Text = "App se validate access token local truoc khi vao he thong.",
            FontSize = 14,
            TextColor = Color.FromArgb("#6B7280"),
            LineBreakMode = LineBreakMode.WordWrap
        };

        _loading = new ActivityIndicator
        {
            IsRunning = true,
            Color = Color.FromArgb("#F97316"),
            HorizontalOptions = LayoutOptions.Start
        };

        _optionsLayout = new VerticalStackLayout
        {
            Spacing = 14,
            IsVisible = false,
            Children =
            {
                BuildOptionCard(
                    "Quet QR",
                    "Quet ma QR tai thiet bi de nhan access token va mo khoa noi dung.",
                    async () =>
                    {
                        var qrPage = App.Current?.Handler?.MauiContext?.Services.GetRequiredService<QrScanPage>();
                        if (qrPage != null)
                            await Navigation.PushAsync(qrPage);
                    }),
                BuildOptionCard(
                    "Dang ky goi",
                    "Chon goi dich vu, den trang thanh toan QR, tick bypass de kich hoat va nhan QR recovery qua email.",
                    async () =>
                    {
                        var packagePage = App.Current?.Handler?.MauiContext?.Services.GetRequiredService<PackageRegistrationPage>();
                        if (packagePage != null)
                            await Navigation.PushAsync(packagePage);
                    })
            }
        };

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 20,
                Padding = new Thickness(16, 64, 16, 28),
                Children =
                {
                    new Border
                    {
                        StrokeThickness = 0,
                        BackgroundColor = Color.FromArgb("#FFF1E6"),
                        StrokeShape = new RoundRectangle { CornerRadius = 999 },
                        Padding = new Thickness(12, 7),
                        HorizontalOptions = LayoutOptions.Start,
                        Content = new Label
                        {
                            Text = "Access Flow",
                            FontSize = 12,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Color.FromArgb("#9A3412")
                        }
                    },
                    _statusLabel,
                    _subtitleLabel,
                    _loading,
                    _optionsLayout
                }
            }
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isChecking)
            return;

        _isChecking = true;
        await CheckAccessAsync();
        _isChecking = false;
    }

    private async Task CheckAccessAsync()
    {
        _loading.IsVisible = true;
        _loading.IsRunning = true;
        _optionsLayout.IsVisible = false;
        _statusLabel.Text = "Du khach mo app";
        _subtitleLabel.Text = "Dang kiem tra accessToken trong local va validate phien truy cap.";

        var validation = await _accessFlowService.ValidateCurrentAccessAsync();
        if (validation.IsValid)
        {
            _statusLabel.Text = "Token hop le";
            _subtitleLabel.Text = validation.ExpiresAtUtc.HasValue
                ? $"Token con han den {validation.ExpiresAtUtc.Value.ToLocalTime():dd/MM/yyyy HH:mm}."
                : "Token hop le, dang vao chuc nang chinh.";

            await Task.Delay(450);
            if (Application.Current is App app)
                await app.ShowMainPageAsync();
            return;
        }

        _loading.IsRunning = false;
        _loading.IsVisible = false;
        _statusLabel.Text = "Chon cach truy cap";
        _subtitleLabel.Text = string.IsNullOrWhiteSpace(validation.Message)
            ? "Ban co the quet QR hoac dang ky goi dich vu de vao app."
            : $"{validation.Message} Ban co the quet QR hoac dang ky goi dich vu de vao app.";
        _optionsLayout.IsVisible = true;
    }

    private static View BuildOptionCard(string title, string description, Func<Task> onTap)
    {
        var card = new Border
        {
            StrokeThickness = 1,
            Stroke = Color.FromArgb("#F0E6DC"),
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            Padding = new Thickness(16),
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Label
                    {
                        Text = title,
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#111111")
                    },
                    new Label
                    {
                        Text = description,
                        FontSize = 13,
                        TextColor = Color.FromArgb("#6B7280"),
                        LineBreakMode = LineBreakMode.WordWrap
                    }
                }
            }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, __) => await onTap();
        card.GestureRecognizers.Add(tap);

        return card;
    }
}
