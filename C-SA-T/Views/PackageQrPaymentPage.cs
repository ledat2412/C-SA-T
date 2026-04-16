using Microsoft.Maui.Controls.Shapes;
using MauiApp1.Models;
using MauiApp1.Services;
using ZXing.Net.Maui.Controls;
using MauiColor = Microsoft.Maui.Graphics.Color;

namespace MauiApp1.Views;

public class PackageQrPaymentPage : ContentPage
{
    private readonly AccessFlowService _accessFlowService;
    private readonly LocalizationService _loc;
    private readonly PackagePlanOption _plan;
    private readonly string _email;
    private readonly CheckBox _bypassCheckBox;
    private readonly Button _bypassButton;
    private readonly Label _statusLabel;
    private readonly Label _helperLabel;
    private readonly VerticalStackLayout _successLayout;
    private bool _isSubmitting;

    public PackageQrPaymentPage(AccessFlowService accessFlowService, LocalizationService localizationService, PackagePlanOption plan, string email)
    {
        _accessFlowService = accessFlowService;
        _loc = localizationService;
        _plan = plan;
        _email = email;

        NavigationPage.SetHasNavigationBar(this, false);
        BackgroundColor = MauiColor.FromArgb("#FFF7F1");
        Title = string.Empty;

        _statusLabel = new Label
        {
            Text = GetText("status_waiting"),
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = MauiColor.FromArgb("#9A3412")
        };

        _helperLabel = new Label
        {
            Text = GetText("helper_waiting"),
            FontSize = 13,
            TextColor = MauiColor.FromArgb("#6B7280"),
            LineBreakMode = LineBreakMode.WordWrap
        };

        _bypassButton = new Button
        {
            Text = GetText("bypass_button"),
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
                Text = GetText("back"),
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
                        Text = GetText("title"),
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
                                    Text = string.Format(GetText("email_line"), _email),
                                    FontSize = 13,
                                    TextColor = MauiColor.FromArgb("#6B7280")
                                },
                                new Label
                                {
                                    Text = string.Format(GetText("price_line"), _plan.Price, _plan.DurationDays),
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
                                    Text = GetText("payment_qr"),
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
                                            Text = GetText("bypass_hint"),
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
            _statusLabel.Text = GetText("status_generating");
            _helperLabel.Text = GetText("helper_generating");

            var result = await _accessFlowService.RegisterPackageAccessBypassAsync(_email, _plan.BackendPackageId);
            if (!result.Success)
            {
                _statusLabel.Text = GetText("status_failed");
                _helperLabel.Text = result.Message;
                await DisplayAlertAsync(GetText("failed_title"), result.Message, _loc.Get("alert_ok"));
                return;
            }

            _statusLabel.Text = GetText("status_activated");
            _helperLabel.Text = result.EmailSent
                ? GetText("helper_email_sent")
                : result.EmailStatusMessage ?? GetText("helper_qr_ready");

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
            Text = result.EmailSent ? GetText("gmail_sent") : GetText("gmail_open"),
            BackgroundColor = Colors.White,
            TextColor = MauiColor.FromArgb("#0F766E"),
            BorderColor = MauiColor.FromArgb("#CCFBF1"),
            BorderWidth = 1,
            CornerRadius = 14,
            IsEnabled = !result.EmailSent
        };
        gmailButton.Clicked += async (_, __) =>
        {
            var subject = Uri.EscapeDataString(GetText("gmail_subject"));
            var expiresText = result.ExpiresAtUtc?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? string.Empty;
            var body = Uri.EscapeDataString(
                $"{string.Format(GetText("email_line"), result.Email)}\n{string.Format(GetText("package_line"), result.PackageName)}\n{string.Format(GetText("token_line"), result.AccessToken)}\n{string.Format(GetText("payload_line"), result.QrTokenPayload)}\n{string.Format(GetText("expires_line"), expiresText)}");
            await Launcher.Default.OpenAsync(new Uri($"mailto:{result.Email}?subject={subject}&body={body}"));
        };

        var enterButton = new Button
        {
            Text = GetText("enter_app"),
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
                            Text = GetText("success_title"),
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = MauiColor.FromArgb("#111111")
                        },
                        new Label
                        {
                            Text = $"{string.Format(GetText("package_line"), result.PackageName)}\n{string.Format(GetText("email_line"), result.Email)}\n{string.Format(GetText("expires_line"), result.ExpiresAtUtc?.ToLocalTime().ToString("dd/MM/yyyy HH:mm") ?? string.Empty)}",
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
                            Text = string.Format(GetText("payload_line"), result.QrTokenPayload),
                            FontSize = 12,
                            TextColor = MauiColor.FromArgb("#334155"),
                            LineBreakMode = LineBreakMode.WordWrap
                        },
                        new Label
                        {
                            Text = result.EmailSent
                                ? GetText("email_backend_sent")
                                : (result.EmailStatusMessage ?? GetText("email_manual_hint")),
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

    private string GetText(string key)
    {
        return _loc.CurrentLanguage switch
        {
            "en" => key switch
            {
                "status_waiting" => "Waiting for payment confirmation",
                "helper_waiting" => "The QR payment flow is currently simulated. Check bypass to skip the real payment step.",
                "bypass_button" => "Confirm bypass",
                "back" => "Back",
                "title" => "QR payment",
                "email_line" => "Email: {0}",
                "price_line" => "Price: {0:N0} VND | {1} days",
                "payment_qr" => "Payment QR code",
                "bypass_hint" => "Bypass payment to create the login QR token",
                "status_generating" => "Generating token for the selected package...",
                "helper_generating" => "The system will bypass payment, activate the token on this device, and send the login QR token by email.",
                "status_failed" => "Bypass failed",
                "failed_title" => "Failed",
                "status_activated" => "Token activated",
                "helper_email_sent" => "The token has been activated and the login QR token email has been sent.",
                "helper_qr_ready" => "The token has been activated and the login QR token is ready.",
                "gmail_sent" => "Email sent",
                "gmail_open" => "Open Gmail to send the QR token",
                "gmail_subject" => "Vinh Khanh Smart Tourism login QR token",
                "package_line" => "Package: {0}",
                "token_line" => "Token: {0}",
                "payload_line" => "QR payload: {0}",
                "expires_line" => "Expires: {0}",
                "enter_app" => "Enter app",
                "success_title" => "Package activated successfully",
                "email_backend_sent" => "The backend sent the QR token email automatically.",
                "email_manual_hint" => "Automatic email could not be sent. You can use the Gmail button to send it manually.",
                _ => key
            },
            "ko" => key switch
            {
                "status_waiting" => "결제 확인 대기 중",
                "helper_waiting" => "현재 QR 결제 흐름은 시뮬레이션입니다. 실제 결제를 건너뛰려면 bypass를 체크하세요.",
                "bypass_button" => "bypass 확인",
                "back" => "뒤로",
                "title" => "QR 결제",
                "email_line" => "이메일: {0}",
                "price_line" => "가격: {0:N0} VND | {1}일",
                "payment_qr" => "결제 QR 코드",
                "bypass_hint" => "로그인 QR 토큰을 만들기 위해 결제를 bypass 합니다",
                "status_generating" => "선택한 패키지의 토큰을 생성하는 중...",
                "helper_generating" => "시스템이 결제를 bypass 하고, 이 기기에서 토큰을 활성화한 뒤 이메일로 로그인 QR 토큰을 보냅니다.",
                "status_failed" => "bypass 실패",
                "failed_title" => "실패",
                "status_activated" => "토큰 활성화 완료",
                "helper_email_sent" => "토큰이 활성화되었고 로그인 QR 토큰 이메일이 전송되었습니다.",
                "helper_qr_ready" => "토큰이 활성화되었고 로그인 QR 토큰이 준비되었습니다.",
                "gmail_sent" => "이메일 전송 완료",
                "gmail_open" => "Gmail을 열어 QR 토큰 보내기",
                "gmail_subject" => "Vinh Khanh Smart Tourism 로그인 QR 토큰",
                "package_line" => "패키지: {0}",
                "token_line" => "토큰: {0}",
                "payload_line" => "QR payload: {0}",
                "expires_line" => "만료: {0}",
                "enter_app" => "앱으로 이동",
                "success_title" => "패키지 활성화 완료",
                "email_backend_sent" => "백엔드가 QR 토큰 이메일을 자동으로 보냈습니다.",
                "email_manual_hint" => "자동 이메일 전송에 실패했습니다. Gmail 버튼으로 수동 발송할 수 있습니다.",
                _ => key
            },
            "ja" => key switch
            {
                "status_waiting" => "支払い確認待ち",
                "helper_waiting" => "現在のQR決済フローはシミュレーションです。実際の支払いを飛ばすには bypass をチェックしてください。",
                "bypass_button" => "bypass を確認",
                "back" => "戻る",
                "title" => "QR決済",
                "email_line" => "メール: {0}",
                "price_line" => "価格: {0:N0} VND | {1}日",
                "payment_qr" => "決済QRコード",
                "bypass_hint" => "ログイン用QRトークンを作るために支払いを bypass します",
                "status_generating" => "選択したプランのトークンを生成中...",
                "helper_generating" => "システムは支払いを bypass し、この端末でトークンを有効化してからログイン用QRトークンをメール送信します。",
                "status_failed" => "bypass 失敗",
                "failed_title" => "失敗",
                "status_activated" => "トークン有効化完了",
                "helper_email_sent" => "トークンを有効化し、ログイン用QRトークンのメールを送信しました。",
                "helper_qr_ready" => "トークンを有効化し、ログイン用QRトークンを生成しました。",
                "gmail_sent" => "メール送信済み",
                "gmail_open" => "Gmail を開いてQRトークンを送信",
                "gmail_subject" => "Vinh Khanh Smart Tourism ログイン用QRトークン",
                "package_line" => "プラン: {0}",
                "token_line" => "トークン: {0}",
                "payload_line" => "QR payload: {0}",
                "expires_line" => "有効期限: {0}",
                "enter_app" => "アプリに入る",
                "success_title" => "プランの有効化に成功しました",
                "email_backend_sent" => "バックエンドがQRトークンメールを自動送信しました。",
                "email_manual_hint" => "自動メール送信に失敗しました。Gmail ボタンから手動送信できます。",
                _ => key
            },
            _ => key switch
            {
                "status_waiting" => "Cho xac nhan thanh toan",
                "helper_waiting" => "Luong QR thanh toan hien dang mo phong. Hay tick bypass de bo qua buoc thanh toan that.",
                "bypass_button" => "Xac thuc bypass",
                "back" => "Quay lai",
                "title" => "Thanh toan QR",
                "email_line" => "Email: {0}",
                "price_line" => "Gia: {0:N0} VND | {1} ngay",
                "payment_qr" => "Ma QR thanh toan",
                "bypass_hint" => "Bypass thanh toan de tao QR token dang nhap",
                "status_generating" => "Dang sinh token theo goi dich vu...",
                "helper_generating" => "He thong se bypass thanh toan, kich hoat token tren may nay va gui QR token den email.",
                "status_failed" => "Bypass that bai",
                "failed_title" => "That bai",
                "status_activated" => "Da kich hoat token",
                "helper_email_sent" => "Da kich hoat token va gui email QR token dang nhap.",
                "helper_qr_ready" => "Da kich hoat token va sinh QR token dang nhap.",
                "gmail_sent" => "Email da gui",
                "gmail_open" => "Mo Gmail de gui QR token",
                "gmail_subject" => "QR token dang nhap Vinh Khanh Smart Tourism",
                "package_line" => "Goi: {0}",
                "token_line" => "Token: {0}",
                "payload_line" => "QR payload: {0}",
                "expires_line" => "Het han: {0}",
                "enter_app" => "Vao app",
                "success_title" => "Da kich hoat goi thanh cong",
                "email_backend_sent" => "Backend da gui email QR token tu dong.",
                "email_manual_hint" => "Chua gui duoc email tu dong, ban co the dung nut Gmail de gui thu cong.",
                _ => key
            }
        };
    }
}
