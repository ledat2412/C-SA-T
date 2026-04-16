using Microsoft.Maui.Controls.Shapes;
using MauiApp1.Models;
using MauiApp1.Services;
using ZXing.Net.Maui.Controls;
using MauiColor = Microsoft.Maui.Graphics.Color;

namespace MauiApp1.Views;

public class PackageQrPaymentPage : ContentPage
{
    private readonly AccessFlowService _accessFlowService;
    private readonly PackagePlanOption _plan;
    private readonly string _email;
    private readonly CheckBox _bypassCheckBox;
    private readonly Button _bypassButton;
    private readonly Label _statusLabel;
    private readonly Label _helperLabel;
    private readonly VerticalStackLayout _successLayout;
    private bool _isSubmitting;

    public PackageQrPaymentPage(AccessFlowService accessFlowService, PackagePlanOption plan, string email)
    {
        _accessFlowService = accessFlowService;
        _plan = plan;
        _email = email;

        NavigationPage.SetHasNavigationBar(this, false);
        BackgroundColor = MauiColor.FromArgb("#FFF7F1");
        Title = string.Empty;

        _statusLabel = new Label
        {
            Text = "Cho xac nhan thanh toan",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = MauiColor.FromArgb("#9A3412")
        };

        _helperLabel = new Label
        {
            Text = "Luong QR thanh toan hien dang mo phong. Hay tick bypass de bo qua buoc thanh toan that.",
            FontSize = 13,
            TextColor = MauiColor.FromArgb("#6B7280"),
            LineBreakMode = LineBreakMode.WordWrap
        };

        _bypassButton = new Button
        {
            Text = "Xac thuc bypass",
            BackgroundColor = MauiColor.FromArgb("#F97316"),
            TextColor = Colors.White,
            CornerRadius = 16,
            Padding = new Thickness(14, 12),
            IsVisible = false
        };
        _bypassButton.Clicked += async (_, __) => await ConfirmBypassAsync();

        _bypassCheckBox = new CheckBox
        {
            Color = MauiColor.FromArgb("#F97316")
        };
        _bypassCheckBox.CheckedChanged += (_, args) =>
        {
            _bypassButton.IsVisible = args.Value;
        };

        _successLayout = new VerticalStackLayout
        {
            Spacing = 12,
            IsVisible = false
        };

        var backButton = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness(14, 10),
            Content = new Label
            {
                Text = "Quay lai",
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = MauiColor.FromArgb("#334155")
            }
        };
        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, __) => await Navigation.PopAsync();
        backButton.GestureRecognizers.Add(tap);

        var paymentPayload = $"PAYQR|goi={_plan.BackendPackageId}|email={_email}|gia={_plan.Price:0}";

        Content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 18,
                Padding = new Thickness(16, 52, 16, 28),
                Children =
                {
                    backButton,
                    new Label
                    {
                        Text = "Thanh toan QR",
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = MauiColor.FromArgb("#111111")
                    },
                    BuildCard(
                        new VerticalStackLayout
                        {
                            Spacing = 8,
                            Children =
                            {
                                new Label
                                {
                                    Text = _plan.Name,
                                    FontSize = 18,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = MauiColor.FromArgb("#111111")
                                },
                                new Label
                                {
                                    Text = $"Email: {_email}",
                                    FontSize = 13,
                                    TextColor = MauiColor.FromArgb("#6B7280")
                                },
                                new Label
                                {
                                    Text = $"Gia: {_plan.Price:N0} VND | {_plan.DurationDays} ngay",
                                    FontSize = 13,
                                    TextColor = MauiColor.FromArgb("#6B7280")
                                }
                            }
                        }),
                    BuildCard(
                        new VerticalStackLayout
                        {
                            Spacing = 12,
                            Children =
                            {
                                new Label
                                {
                                    Text = "Ma QR thanh toan",
                                    FontSize = 15,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = MauiColor.FromArgb("#111111")
                                },
                                new Border
                                {
                                    StrokeThickness = 1,
                                    Stroke = MauiColor.FromArgb("#F0E6DC"),
                                    BackgroundColor = Colors.White,
                                    StrokeShape = new RoundRectangle { CornerRadius = 18 },
                                    Padding = new Thickness(18),
                                    HorizontalOptions = LayoutOptions.Center,
                                    Content = new BarcodeGeneratorView
                                    {
                                        Value = paymentPayload,
                                        Format = ZXing.Net.Maui.BarcodeFormat.QrCode,
                                        WidthRequest = 220,
                                        HeightRequest = 220,
                                        ForegroundColor = Colors.Black
                                    }
                                },
                                _statusLabel,
                                _helperLabel,
                                new HorizontalStackLayout
                                {
                                    Spacing = 10,
                                    Children =
                                    {
                                        _bypassCheckBox,
                                        new Label
                                        {
                                            Text = "Bypass thanh toan de tao QR token dang nhap",
                                            FontSize = 13,
                                            TextColor = MauiColor.FromArgb("#334155"),
                                            VerticalTextAlignment = TextAlignment.Center
                                        }
                                    }
                                },
                                _bypassButton
                            }
                        }),
                    _successLayout
                }
            }
        };
    }

    private async Task ConfirmBypassAsync()
    {
        if (_isSubmitting)
            return;

        _isSubmitting = true;
        _bypassButton.IsEnabled = false;

        try
        {
            _statusLabel.Text = "Dang sinh token theo goi dich vu...";
            _helperLabel.Text = "He thong se bypass thanh toan, kich hoat token tren may nay va gui QR token den email.";

            var result = await _accessFlowService.RegisterPackageAccessBypassAsync(_email, _plan.BackendPackageId);
            if (!result.Success)
            {
                _statusLabel.Text = "Bypass that bai";
                _helperLabel.Text = result.Message;
                await DisplayAlertAsync("That bai", result.Message, "OK");
                return;
            }

            _statusLabel.Text = "Da kich hoat token";
            _helperLabel.Text = result.EmailSent
                ? "Da kich hoat token va gui email QR token dang nhap."
                : result.EmailStatusMessage ?? "Da kich hoat token va sinh QR token dang nhap.";

            ShowSuccess(result);
        }
        finally
        {
            _bypassButton.IsEnabled = true;
            _isSubmitting = false;
        }
    }

    private void ShowSuccess(PackageAccessActivationState result)
    {
        _successLayout.Children.Clear();
        _successLayout.IsVisible = true;

        var gmailButton = new Button
        {
            Text = result.EmailSent ? "Email da gui" : "Mo Gmail de gui QR token",
            BackgroundColor = Colors.White,
            TextColor = MauiColor.FromArgb("#0F766E"),
            BorderColor = MauiColor.FromArgb("#CCFBF1"),
            BorderWidth = 1,
            CornerRadius = 14,
            IsEnabled = !result.EmailSent
        };
        gmailButton.Clicked += async (_, __) =>
        {
            var subject = Uri.EscapeDataString("QR token dang nhap Vinh Khanh Smart Tourism");
            var body = Uri.EscapeDataString(
                $"Email: {result.Email}\nGoi: {result.PackageName}\nToken: {result.AccessToken}\nQR payload: {result.QrTokenPayload}\nHet han: {result.ExpiresAtUtc?.ToLocalTime():dd/MM/yyyy HH:mm}");
            await Launcher.Default.OpenAsync(new Uri($"mailto:{result.Email}?subject={subject}&body={body}"));
        };

        var enterButton = new Button
        {
            Text = "Vao app",
            BackgroundColor = MauiColor.FromArgb("#F97316"),
            TextColor = Colors.White,
            CornerRadius = 16
        };
        enterButton.Clicked += async (_, __) =>
        {
            if (Application.Current is App app)
                await app.ShowMainPageAsync();
        };

        _successLayout.Children.Add(
            BuildCard(
                new VerticalStackLayout
                {
                    Spacing = 12,
                    Children =
                    {
                        new Label
                        {
                            Text = "Da kich hoat goi thanh cong",
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = MauiColor.FromArgb("#111111")
                        },
                        new Label
                        {
                            Text = $"Goi: {result.PackageName}\nEmail: {result.Email}\nHet han: {result.ExpiresAtUtc?.ToLocalTime():dd/MM/yyyy HH:mm}",
                            FontSize = 13,
                            TextColor = MauiColor.FromArgb("#6B7280")
                        },
                        new Border
                        {
                            StrokeThickness = 1,
                            Stroke = MauiColor.FromArgb("#F0E6DC"),
                            BackgroundColor = Colors.White,
                            StrokeShape = new RoundRectangle { CornerRadius = 18 },
                            Padding = new Thickness(18),
                            HorizontalOptions = LayoutOptions.Center,
                            Content = new BarcodeGeneratorView
                            {
                                Value = result.QrTokenPayload ?? result.AccessToken,
                                Format = ZXing.Net.Maui.BarcodeFormat.QrCode,
                                WidthRequest = 220,
                                HeightRequest = 220,
                                ForegroundColor = Colors.Black
                            }
                        },
                        new Label
                        {
                            Text = $"QR payload: {result.QrTokenPayload}",
                            FontSize = 12,
                            TextColor = MauiColor.FromArgb("#334155"),
                            LineBreakMode = LineBreakMode.WordWrap
                        },
                        new Label
                        {
                            Text = result.EmailSent
                                ? "Backend da gui email QR token tu dong."
                                : (result.EmailStatusMessage ?? "Chua gui duoc email tu dong, ban co the dung nut Gmail de gui thu cong."),
                            FontSize = 12,
                            TextColor = MauiColor.FromArgb("#9A3412"),
                            LineBreakMode = LineBreakMode.WordWrap
                        },
                        gmailButton,
                        enterButton
                    }
                }));
    }

    private static View BuildCard(View content)
    {
        return new Border
        {
            StrokeThickness = 1,
            Stroke = MauiColor.FromArgb("#F0E6DC"),
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            Padding = new Thickness(16, 14),
            Content = content
        };
    }
}
