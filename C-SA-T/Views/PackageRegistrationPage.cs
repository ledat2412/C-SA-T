using Microsoft.Maui.Controls.Shapes;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiColor = Microsoft.Maui.Graphics.Color;

namespace MauiApp1.Views;

public class PackageRegistrationPage : ContentPage
{
    private readonly AccessFlowService _accessFlowService;
    private readonly Entry _emailEntry;
    private readonly VerticalStackLayout _packageList;
    private readonly Button _paymentButton;
    private readonly Label _selectedPlanLabel;
    private readonly Label _helperLabel;
    private PackagePlanOption? _selectedPlan;

    private readonly List<PackagePlanOption> _plans =
    [
        new(1, "Goi ngay", "1 ngay truy cap cho du khach.", 1, 15000m, true),
        new(2, "Goi 7 ngay", "7 ngay truy cap bang QR token dang nhap.", 7, 70000m, true),
        new(3, "Goi thang", "30 ngay truy cap bang QR token dang nhap.", 30, 500000m, true)
    ];

    public PackageRegistrationPage(AccessFlowService accessFlowService)
    {
        _accessFlowService = accessFlowService;

        NavigationPage.SetHasNavigationBar(this, false);
        BackgroundColor = MauiColor.FromArgb("#FFF7F1");
        Title = string.Empty;

        _emailEntry = new Entry
        {
            Placeholder = "Nhap gmail de nhan QR token dang nhap",
            Keyboard = Keyboard.Email,
            BackgroundColor = Colors.White,
            TextColor = MauiColor.FromArgb("#111111"),
            PlaceholderColor = MauiColor.FromArgb("#94A3B8")
        };
        _emailEntry.TextChanged += (_, __) => RefreshPaymentButton();

        _selectedPlanLabel = new Label
        {
            Text = "Chua chon goi nao.",
            FontSize = 13,
            TextColor = MauiColor.FromArgb("#9A3412")
        };

        _helperLabel = new Label
        {
            Text = "Chon mot goi, nhap email hop le, sau do bam Thanh toan de sang trang QR thanh toan.",
            FontSize = 13,
            TextColor = MauiColor.FromArgb("#6B7280"),
            LineBreakMode = LineBreakMode.WordWrap
        };

        _packageList = new VerticalStackLayout { Spacing = 12 };
        RenderPackages();

        _paymentButton = new Button
        {
            Text = "Thanh toan",
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
                Text = "Quay lai",
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
                        Text = "Dang ky goi truy cap",
                        FontSize = 28,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = MauiColor.FromArgb("#111111")
                    },
                    new Label
                    {
                        Text = "Chon goi dich vu, nhap gmail, sau do bam Thanh toan de den trang thanh toan QR.",
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
                                    Text = "Email nhan QR token dang nhap",
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
                                    Text = "Danh sach goi dich vu",
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
                            Text = $"{plan.DurationDays} ngay | {plan.Price:N0} VND",
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
                _selectedPlanLabel.Text = $"Da chon: {plan.Name}";
                _helperLabel.Text = "Bam Thanh toan de mo trang QR thanh toan. Tai trang tiep theo ban co the tick bypass de tao QR token dang nhap.";
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
            await DisplayAlertAsync("Thong bao", "Vui long nhap gmail hop le.", "OK");
            return;
        }

        await Navigation.PushAsync(new PackageQrPaymentPage(_accessFlowService, _selectedPlan, email));
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
}
