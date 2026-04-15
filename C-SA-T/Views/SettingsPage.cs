using MauiApp1.Controls;
using MauiApp1.Services;
using Microsoft.Maui.Controls.Shapes;

namespace MauiApp1.Views;

public class SettingsPage : ContentPage
{
    private readonly LocalizationService _loc;
    private Label _titleLabel = null!;
    private Label _langSectionLabel = null!;
    private HorizontalStackLayout _chipGrid = null!;

    private static readonly (string Code, string NativeName)[] _languages =
    {
        ("vi", "Tiếng Việt"),
        ("en", "English"),
        ("ko", "한국어"),
        ("ja", "日本語")
    };

    public SettingsPage(LocalizationService localizationService)
    {
        _loc = localizationService;

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
                Children = { _titleLabel, languageCard }
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
        RenderLanguageChips();
    }
}
