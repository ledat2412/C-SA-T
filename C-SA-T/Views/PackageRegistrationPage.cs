using Microsoft.Maui.Controls.Shapes;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiColor = Microsoft.Maui.Graphics.Color;

namespace MauiApp1.Views;

public class PackageRegistrationPage : ContentPage
{
    private readonly AccessFlowService _accessFlowService;
    private readonly LocalizationService _loc;
    private readonly Entry _emailEntry;
    private readonly VerticalStackLayout _packageList;
    private readonly Button _paymentButton;
    private readonly Label _selectedPlanLabel;
    private readonly Label _helperLabel;
    private PackagePlanOption? _selectedPlan;
    private readonly List<PackagePlanOption> _plans;

    public PackageRegistrationPage(AccessFlowService accessFlowService, LocalizationService localizationService)
    {
        _accessFlowService = accessFlowService;
        _loc = localizationService;
        _plans = BuildPlans();

        NavigationPage.SetHasNavigationBar(this, false);
        BackgroundColor = MauiColor.FromArgb("#FFF7F1");
        Title = string.Empty;

        _emailEntry = new Entry
        {
            Placeholder = GetText("email_placeholder"),
            Keyboard = Keyboard.Email,
            BackgroundColor = Colors.White,
            TextColor = MauiColor.FromArgb("#111111"),
            PlaceholderColor = MauiColor.FromArgb("#94A3B8")
        };
        _emailEntry.TextChanged += (_, __) => RefreshPaymentButton();

        _selectedPlanLabel = new Label
        {
            Text = GetText("no_plan"),
            FontSize = 13,
            TextColor = MauiColor.FromArgb("#9A3412")
        };

        _helperLabel = new Label
        {
            Text = GetText("helper_default"),
            FontSize = 13,
            TextColor = MauiColor.FromArgb("#6B7280"),
            LineBreakMode = LineBreakMode.WordWrap
        };

        _packageList = new VerticalStackLayout { Spacing = 12 };
        RenderPackages();

        _paymentButton = new Button
        {
            Text = GetText("pay"),
            BackgroundColor = MauiColor.FromArgb("#F97316"),
            TextColor = Colors.White,
            CornerRadius = 16,
            Padding = new Thickness(14, 12),
            IsVisible = false
        };
        _paymentButton.Clicked += async (_, __) => await GoToPaymentAsync();

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

        var scrollView = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 18,
                Padding = new Thickness(16, 52, 16, 20),
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
                    new Label
                    {
                        Text = GetText("subtitle"),
                        FontSize = 14,
                        TextColor = MauiColor.FromArgb("#6B7280"),
                        LineBreakMode = LineBreakMode.WordWrap
                    },
                    BuildCard(
                        new VerticalStackLayout
                        {
                            Spacing = 10,
                            Children =
                            {
                                new Label
                                {
                                    Text = GetText("email_section"),
                                    FontSize = 15,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = MauiColor.FromArgb("#111111")
                                },
                                _emailEntry
                            }
                        }),
                    BuildCard(
                        new VerticalStackLayout
                        {
                            Spacing = 10,
                            Children =
                            {
                                new Label
                                {
                                    Text = GetText("plan_section"),
                                    FontSize = 15,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = MauiColor.FromArgb("#111111")
                                },
                                _packageList,
                                _selectedPlanLabel,
                                _helperLabel
                            }
                        })
                }
            }
        };

        var footer = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Colors.White,
            Padding = new Thickness(16, 12, 16, 18),
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.05f,
                Radius = 12,
                Offset = new Point(0, -2)
            },
            Content = _paymentButton
        };

        var rootGrid = new Grid();
        rootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
        rootGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
        rootGrid.Children.Add(scrollView);
        rootGrid.Children.Add(footer);
        rootGrid.SetRow(scrollView, 0);
        rootGrid.SetRow(footer, 1);

        Content = rootGrid;
    }

    private void RenderPackages()
    {
        _packageList.Children.Clear();

        foreach (var plan in _plans)
        {
            var isSelected = _selectedPlan?.BackendPackageId == plan.BackendPackageId;

            var card = new Border
            {
                StrokeThickness = 1,
                Stroke = isSelected ? MauiColor.FromArgb("#F97316") : MauiColor.FromArgb("#E5E7EB"),
                BackgroundColor = isSelected ? MauiColor.FromArgb("#FFF7ED") : Colors.White,
                StrokeShape = new RoundRectangle { CornerRadius = 18 },
                Padding = new Thickness(14),
                Content = new VerticalStackLayout
                {
                    Spacing = 5,
                    Children =
                    {
                        new Label
                        {
                            Text = plan.Name,
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = MauiColor.FromArgb("#111111")
                        },
                        new Label
                        {
                            Text = plan.Description,
                            FontSize = 13,
                            TextColor = MauiColor.FromArgb("#6B7280"),
                            LineBreakMode = LineBreakMode.WordWrap
                        },
                        new Label
                        {
                            Text = string.Format(GetText("plan_meta"), plan.DurationDays, plan.Price),
                            FontSize = 12,
                            TextColor = isSelected ? MauiColor.FromArgb("#9A3412") : MauiColor.FromArgb("#64748B")
                        }
                    }
                }
            };

            var gesture = new TapGestureRecognizer();
            gesture.Tapped += (_, __) =>
            {
                if (!plan.IsEnabled)
                    return;

                _selectedPlan = plan;
                _selectedPlanLabel.Text = string.Format(GetText("selected_plan"), plan.Name);
                _helperLabel.Text = GetText("helper_selected");
                RenderPackages();
                RefreshPaymentButton();
            };
            card.GestureRecognizers.Add(gesture);

            _packageList.Children.Add(card);
        }
    }

    private void RefreshPaymentButton()
    {
        _paymentButton.IsVisible = _selectedPlan is not null && IsValidEmail(_emailEntry.Text);
    }

    private async Task GoToPaymentAsync()
    {
        if (_selectedPlan is null)
            return;

        var email = _emailEntry.Text?.Trim() ?? string.Empty;
        if (!IsValidEmail(email))
        {
            await DisplayAlertAsync(GetText("notice"), GetText("invalid_email"), _loc.Get("alert_ok"));
            return;
        }

        await Navigation.PushAsync(new PackageQrPaymentPage(_accessFlowService, _loc, _selectedPlan, email));
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

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return email.Contains('@') && email.Contains('.');
    }

    private List<PackagePlanOption> BuildPlans()
    {
        return _loc.CurrentLanguage switch
        {
            "en" =>
            [
                new(1, "Day pass", "1 day of access for visitors.", 1, 15000m, true),
                new(2, "7-day pass", "7 days of access with a QR login token.", 7, 70000m, true),
                new(3, "Monthly pass", "30 days of access with a QR login token.", 30, 500000m, true)
            ],
            "ko" =>
            [
                new(1, "1일권", "방문객용 1일 이용권입니다.", 1, 15000m, true),
                new(2, "7일권", "QR 로그인 토큰으로 7일 동안 이용할 수 있습니다.", 7, 70000m, true),
                new(3, "월간권", "QR 로그인 토큰으로 30일 동안 이용할 수 있습니다.", 30, 500000m, true)
            ],
            "ja" =>
            [
                new(1, "1日パス", "訪問者向けの1日アクセスです。", 1, 15000m, true),
                new(2, "7日パス", "QRログイントークンで7日間アクセスできます。", 7, 70000m, true),
                new(3, "月間パス", "QRログイントークンで30日間アクセスできます。", 30, 500000m, true)
            ],
            _ =>
            [
                new(1, "Goi ngay", "1 ngay truy cap cho du khach.", 1, 15000m, true),
                new(2, "Goi 7 ngay", "7 ngay truy cap bang QR token dang nhap.", 7, 70000m, true),
                new(3, "Goi thang", "30 ngay truy cap bang QR token dang nhap.", 30, 500000m, true)
            ]
        };
    }

    private string GetText(string key)
    {
        return _loc.CurrentLanguage switch
        {
            "en" => key switch
            {
                "email_placeholder" => "Enter the Gmail address for receiving the login QR token",
                "no_plan" => "No package selected yet.",
                "helper_default" => "Choose a package, enter a valid email, then tap Pay to open the QR payment page.",
                "helper_selected" => "Tap Pay to open the QR payment page. On the next screen you can use bypass to generate the login QR token.",
                "pay" => "Pay",
                "back" => "Back",
                "title" => "Register access package",
                "subtitle" => "Choose a service package, enter your Gmail address, then tap Pay to open the QR payment page.",
                "email_section" => "Email for receiving the login QR token",
                "plan_section" => "Service packages",
                "plan_meta" => "{0} days | {1:N0} VND",
                "selected_plan" => "Selected: {0}",
                "notice" => "Notice",
                "invalid_email" => "Please enter a valid Gmail address.",
                _ => key
            },
            "ko" => key switch
            {
                "email_placeholder" => "로그인 QR 토큰을 받을 Gmail 주소를 입력하세요",
                "no_plan" => "아직 선택한 패키지가 없습니다.",
                "helper_default" => "패키지를 선택하고 유효한 이메일을 입력한 뒤 결제를 눌러 QR 결제 화면으로 이동하세요.",
                "helper_selected" => "결제를 눌러 QR 결제 화면을 여세요. 다음 화면에서 bypass로 로그인 QR 토큰을 생성할 수 있습니다.",
                "pay" => "결제",
                "back" => "뒤로",
                "title" => "이용 패키지 등록",
                "subtitle" => "서비스 패키지를 선택하고 Gmail 주소를 입력한 뒤 결제를 눌러 QR 결제 화면으로 이동하세요.",
                "email_section" => "로그인 QR 토큰 수신 이메일",
                "plan_section" => "서비스 패키지 목록",
                "plan_meta" => "{0}일 | {1:N0} VND",
                "selected_plan" => "선택됨: {0}",
                "notice" => "알림",
                "invalid_email" => "유효한 Gmail 주소를 입력해 주세요.",
                _ => key
            },
            "ja" => key switch
            {
                "email_placeholder" => "ログイン用QRトークンを受け取るGmailアドレスを入力してください",
                "no_plan" => "まだプランが選択されていません。",
                "helper_default" => "プランを選び、有効なメールアドレスを入力してから、支払いを押してQR決済ページへ進みます。",
                "helper_selected" => "支払いを押すとQR決済ページを開きます。次の画面で bypass を使ってログイン用QRトークンを生成できます。",
                "pay" => "支払い",
                "back" => "戻る",
                "title" => "アクセスプラン登録",
                "subtitle" => "サービスプランを選び、Gmailアドレスを入力してから、支払いを押してQR決済ページへ進みます。",
                "email_section" => "ログイン用QRトークン受信メール",
                "plan_section" => "サービスプラン一覧",
                "plan_meta" => "{0}日 | {1:N0} VND",
                "selected_plan" => "選択中: {0}",
                "notice" => "お知らせ",
                "invalid_email" => "有効なGmailアドレスを入力してください。",
                _ => key
            },
            _ => key switch
            {
                "email_placeholder" => "Nhap gmail de nhan QR token dang nhap",
                "no_plan" => "Chua chon goi nao.",
                "helper_default" => "Chon mot goi, nhap email hop le, sau do bam Thanh toan de sang trang QR thanh toan.",
                "helper_selected" => "Bam Thanh toan de mo trang QR thanh toan. Tai trang tiep theo ban co the tick bypass de tao QR token dang nhap.",
                "pay" => "Thanh toan",
                "back" => "Quay lai",
                "title" => "Dang ky goi truy cap",
                "subtitle" => "Chon goi dich vu, nhap gmail, sau do bam Thanh toan de den trang thanh toan QR.",
                "email_section" => "Email nhan QR token dang nhap",
                "plan_section" => "Danh sach goi dich vu",
                "plan_meta" => "{0} ngay | {1:N0} VND",
                "selected_plan" => "Da chon: {0}",
                "notice" => "Thong bao",
                "invalid_email" => "Vui long nhap gmail hop le.",
                _ => key
            }
        };
    }
}
