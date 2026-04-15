using MauiApp1.Controls;
using MauiApp1.Services;
using Microsoft.Maui.Controls.Shapes;

namespace MauiApp1.Views;

public class SettingsPage : ContentPage
{
    private readonly LocalizationService _loc;
    private readonly AccessFlowService _accessFlowService;
    private Label _titleLabel = null!;
    private Label _langSectionLabel = null!;
    private Label _qrSectionLabel = null!;
    private Label _qrSectionDescLabel = null!;
    private Label _resetSectionLabel = null!;
    private Label _resetSectionDescLabel = null!;
    private Label _deleteTokenSectionLabel = null!;
    private Label _deleteTokenSectionDescLabel = null!;
    private HorizontalStackLayout _chipGrid = null!;

    private static readonly (string Code, string NativeName)[] _languages =
    {
        ("vi", "Tiếng Việt"),
        ("en", "English"),
        ("ko", "한국어"),
        ("ja", "日本語")
    };

    public SettingsPage(LocalizationService localizationService, AccessFlowService accessFlowService)
    {
        _loc = localizationService;
        _accessFlowService = accessFlowService;

        NavigationPage.SetHasNavigationBar(this, false);
        BackgroundColor = Color.FromArgb("#FFF7F1");
        SafeAreaEdges = SafeAreaEdges.None;

        BuildContent();

        localizationService.LanguageChanged += () => MainThread.BeginInvokeOnMainThread(UpdateLocalizedText);
        UpdateLocalizedText();
    }

    private void BuildContent()
    {
        _titleLabel = new Label
        {
            FontSize = 26,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#111111"),
            Margin = new Thickness(0, 0, 0, 4)
        };

        _langSectionLabel = new Label
        {
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#111111")
        };

        _chipGrid = new HorizontalStackLayout
        {
            Spacing = 8
        };

        RenderLanguageChips();

        var languageCard = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Stroke = Color.FromArgb("#F0E6DC"),
            BackgroundColor = Colors.White,
            Padding = new Thickness(16, 14),
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children = { _langSectionLabel, _chipGrid }
            }
        };

        _qrSectionLabel = new Label
        {
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#111111")
        };

        _qrSectionDescLabel = new Label
        {
            FontSize = 13,
            TextColor = Color.FromArgb("#6B7280"),
            LineBreakMode = LineBreakMode.WordWrap
        };

        var qrCard = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Stroke = Color.FromArgb("#F0E6DC"),
            BackgroundColor = Colors.White,
            Padding = new Thickness(16, 14),
            Content = BuildQrAccessRow()
        };

        _resetSectionLabel = new Label
        {
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#111111")
        };

        _resetSectionDescLabel = new Label
        {
            FontSize = 13,
            TextColor = Color.FromArgb("#6B7280"),
            LineBreakMode = LineBreakMode.WordWrap
        };

        var resetCard = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Stroke = Color.FromArgb("#F0E6DC"),
            BackgroundColor = Colors.White,
            Padding = new Thickness(16, 14),
            Content = BuildResetAccessRow()
        };

        _deleteTokenSectionLabel = new Label
        {
            FontSize = 15,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#111111")
        };

        _deleteTokenSectionDescLabel = new Label
        {
            FontSize = 13,
            TextColor = Color.FromArgb("#6B7280"),
            LineBreakMode = LineBreakMode.WordWrap
        };

        var deleteTokenCard = new Border
        {
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Stroke = Color.FromArgb("#F0E6DC"),
            BackgroundColor = Colors.White,
            Padding = new Thickness(16, 14),
            Content = BuildDeleteTokenRow()
        };

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            }
        };

        var scroll = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 20,
                Padding = new Thickness(16, 52, 16, 28),
                Children = { _titleLabel, languageCard, qrCard, resetCard, deleteTokenCard }
            }
        };

        root.Children.Add(scroll);

        var footer = new AppBottomBar(
            BottomBarTab.Settings,
            _loc,
            onHomeTap: async () =>
            {
                var homePage = App.Current?.Handler?.MauiContext?.Services.GetRequiredService<HomePage>();
                if (homePage != null)
                    await Navigation.PushAsync(homePage);
            },
            onExploreTap: async () =>
            {
                var poiMapPage = App.Current?.Handler?.MauiContext?.Services.GetRequiredService<Maps.PoiMapPage>();
                if (poiMapPage != null)
                    await Navigation.PushAsync(poiMapPage);
            });
        root.Children.Add(footer);
        Grid.SetRow(footer, 1);

        Content = root;
    }

    private void RenderLanguageChips()
    {
        _chipGrid.Children.Clear();

        foreach (var (code, nativeName) in _languages)
        {
            var isSelected = code.Equals(_loc.CurrentLanguage, StringComparison.OrdinalIgnoreCase);
            _chipGrid.Children.Add(BuildLanguageChip(code, nativeName, isSelected));
        }
    }

    private View BuildLanguageChip(string code, string nativeName, bool selected)
    {
        var chip = new Border
        {
            Stroke = selected ? Color.FromArgb("#FF6B00") : Color.FromArgb("#E5E7EB"),
            StrokeThickness = 1,
            BackgroundColor = selected ? Color.FromArgb("#FFF1E6") : Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = 999 },
            Padding = new Thickness(14, 8),
            Content = new Label
            {
                Text = nativeName,
                FontSize = 13,
                FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None,
                TextColor = selected ? Color.FromArgb("#9A3412") : Color.FromArgb("#374151")
            }
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += (_, __) => OnLanguageSelected(code);
        chip.GestureRecognizers.Add(tap);

        return chip;
    }

    private void OnLanguageSelected(string code)
    {
        if (code.Equals(_loc.CurrentLanguage, StringComparison.OrdinalIgnoreCase))
            return;

        _loc.SetLanguage(code);
        Preferences.Set("ui_language", code);
        RenderLanguageChips();
    }

    private void UpdateLocalizedText()
    {
        Title = _loc.Get("settings_title");
        _titleLabel.Text = _loc.Get("settings_title");
        _langSectionLabel.Text = _loc.Get("settings_language_title");
        _qrSectionLabel.Text = GetQrLabel();
        _qrSectionDescLabel.Text = GetQrDescription();
        _resetSectionLabel.Text = GetResetLabel();
        _resetSectionDescLabel.Text = GetResetDescription();
        _deleteTokenSectionLabel.Text = GetDeleteTokenLabel();
        _deleteTokenSectionDescLabel.Text = GetDeleteTokenDescription();
        RenderLanguageChips();
    }

    private View BuildQrAccessRow()
    {
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };

        row.Children.Add(new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#FFF1E6"),
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness(12),
            WidthRequest = 48,
            HeightRequest = 48,
            Content = new Label
            {
                Text = "QR",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = Color.FromArgb("#9A3412")
            }
        });

        var textWrap = new VerticalStackLayout
        {
            Spacing = 4,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                _qrSectionLabel,
                _qrSectionDescLabel
            }
        };
        row.Children.Add(textWrap);
        row.SetColumn(textWrap, 1);

        var arrowLabel = new Label
        {
            Text = ">",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#94A3B8"),
            VerticalTextAlignment = TextAlignment.Center
        };
        row.Children.Add(arrowLabel);
        row.SetColumn(arrowLabel, 2);

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, __) =>
        {
            var qrPage = App.Current?.Handler?.MauiContext?.Services.GetRequiredService<QrScanPage>();
            if (qrPage != null)
                await Navigation.PushAsync(qrPage);
        };
        row.GestureRecognizers.Add(tap);

        return row;
    }

    private View BuildResetAccessRow()
    {
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };

        row.Children.Add(new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#FFF1E6"),
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness(12),
            WidthRequest = 48,
            HeightRequest = 48,
            Content = new Label
            {
                Text = "X",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = Color.FromArgb("#9A3412")
            }
        });

        var textWrap = new VerticalStackLayout
        {
            Spacing = 4,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                _resetSectionLabel,
                _resetSectionDescLabel
            }
        };
        row.Children.Add(textWrap);
        row.SetColumn(textWrap, 1);

        var actionLabel = new Label
        {
            Text = "Xoa",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#DC2626"),
            VerticalTextAlignment = TextAlignment.Center
        };
        row.Children.Add(actionLabel);
        row.SetColumn(actionLabel, 2);

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, __) => await ResetAccessAsync();
        row.GestureRecognizers.Add(tap);

        return row;
    }

    private View BuildDeleteTokenRow()
    {
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };

        row.Children.Add(new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#FEF2F2"),
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness(12),
            WidthRequest = 48,
            HeightRequest = 48,
            Content = new Label
            {
                Text = "T",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                TextColor = Color.FromArgb("#B91C1C")
            }
        });

        var textWrap = new VerticalStackLayout
        {
            Spacing = 4,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                _deleteTokenSectionLabel,
                _deleteTokenSectionDescLabel
            }
        };
        row.Children.Add(textWrap);
        row.SetColumn(textWrap, 1);

        var actionLabel = new Label
        {
            Text = "Xoa",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#B91C1C"),
            VerticalTextAlignment = TextAlignment.Center
        };
        row.Children.Add(actionLabel);
        row.SetColumn(actionLabel, 2);

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, __) => await DeleteTokenOnDeviceAsync();
        row.GestureRecognizers.Add(tap);

        return row;
    }

    private async Task ResetAccessAsync()
    {
        var confirmed = await DisplayAlertAsync(
            "Reset truy cap",
            "Xoa token truy cap hien tai de quay lai man hinh chon goi va quet QR?",
            "Xoa token",
            "Huy");

        if (!confirmed)
            return;

        _accessFlowService.ClearAccess();

        if (Application.Current is App app)
            await app.ShowAccessEntryAsync();
    }

    private async Task DeleteTokenOnDeviceAsync()
    {
        var confirmed = await DisplayAlertAsync(
            "Xoa token tren may",
            "Chi xoa access token local tren thiet bi nay de ban test lai luong kich hoat?",
            "Xoa token",
            "Huy");

        if (!confirmed)
            return;

        _accessFlowService.ClearAccess();
        await DisplayAlertAsync("Hoan tat", "Da xoa token tren may nay.", "OK");
    }

    private string GetQrLabel()
    {
        return _loc.CurrentLanguage switch
        {
            "en" => "Scan QR",
            _ => "Quet QR"
        };
    }

    private string GetQrDescription()
    {
        return _loc.CurrentLanguage switch
        {
            "en" => "Open the QR scanner to activate access from a device code.",
            _ => "Mo man hinh quet QR de truy cap nhanh bang ma thiet bi."
        };
    }

    private string GetResetLabel()
    {
        return _loc.CurrentLanguage switch
        {
            "en" => "Reset access",
            _ => "Reset truy cap"
        };
    }

    private string GetResetDescription()
    {
        return _loc.CurrentLanguage switch
        {
            "en" => "Clear the local token so you can test package purchase and recovery again.",
            _ => "Xoa token local de ban test lai luong chon goi, thanh toan va gui email recovery."
        };
    }

    private string GetDeleteTokenLabel()
    {
        return _loc.CurrentLanguage switch
        {
            "en" => "Delete token on device",
            _ => "Xoa token tren may"
        };
    }

    private string GetDeleteTokenDescription()
    {
        return _loc.CurrentLanguage switch
        {
            "en" => "Delete only the local token on this device without changing server data.",
            _ => "Chi xoa token local tren may nay, khong dong vao du lieu token o backend."
        };
    }
}
