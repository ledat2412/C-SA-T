using Microsoft.Maui.Controls.Shapes;

namespace MauiApp1.Controls;

public enum BottomBarTab
{
    Home,
    Explore
}

public sealed class AppBottomBar : ContentView
{
    private readonly Func<Task>? _onHomeTap;
    private readonly Func<Task>? _onExploreTap;

    public AppBottomBar(BottomBarTab activeTab, Func<Task>? onHomeTap = null, Func<Task>? onExploreTap = null)
    {
        ActiveTab = activeTab;
        _onHomeTap = onHomeTap;
        _onExploreTap = onExploreTap;

        HorizontalOptions = LayoutOptions.Fill;
        VerticalOptions = LayoutOptions.End;

        Content = BuildRoot();
    }

    public BottomBarTab ActiveTab { get; }

    private View BuildRoot()
    {
        var tabs = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            Padding = new Thickness(16, 8, 16, 10)
        };

        var home = BuildFooterItem(
            BuildHomeIcon(ActiveTab == BottomBarTab.Home),
            "Trang chủ",
            ActiveTab == BottomBarTab.Home,
            _onHomeTap);

        var explore = BuildFooterItem(
            BuildExploreIcon(ActiveTab == BottomBarTab.Explore),
            "Khám phá",
            ActiveTab == BottomBarTab.Explore,
            _onExploreTap);

        tabs.Children.Add(home);
        tabs.Children.Add(explore);
        Grid.SetColumn(explore, 1);

        return new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24, 24, 0, 0) },
            Content = tabs,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.End,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.08f,
                Radius = 18,
                Offset = new Point(0, -3)
            }
        };
    }

    private static View BuildFooterItem(View icon, string text, bool active, Func<Task>? onTap)
    {
        var content = new VerticalStackLayout
        {
            Spacing = 4,
            HorizontalOptions = LayoutOptions.Center,
            Children =
            {
                new Border
                {
                    StrokeThickness = 0,
                    BackgroundColor = active ? Color.FromArgb("#FFE8E2") : Color.FromArgb("#F8FAFC"),
                    StrokeShape = new RoundRectangle { CornerRadius = 18 },
                    Padding = new Thickness(14, 7),
                    Content = icon
                },
                new Label
                {
                    Text = text,
                    FontSize = 10,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = active ? Color.FromArgb("#DC2626") : Color.FromArgb("#7D7D82"),
                    HorizontalTextAlignment = TextAlignment.Center
                }
            }
        };

        if (onTap is not null)
        {
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, __) => await onTap();
            content.GestureRecognizers.Add(tap);
        }

        return content;
    }

    private static View BuildHomeIcon(bool active)
    {
        var stroke = new SolidColorBrush(active ? Color.FromArgb("#DC2626") : Color.FromArgb("#94A3B8"));

        return new Grid
        {
            WidthRequest = 20,
            HeightRequest = 20,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Line
                {
                    X1 = 4.5,
                    Y1 = 9,
                    X2 = 10,
                    Y2 = 4.5,
                    Stroke = stroke,
                    StrokeThickness = 1.8
                },
                new Line
                {
                    X1 = 10,
                    Y1 = 4.5,
                    X2 = 15.5,
                    Y2 = 9,
                    Stroke = stroke,
                    StrokeThickness = 1.8
                },
                new Line
                {
                    X1 = 5.4,
                    Y1 = 8.7,
                    X2 = 5.4,
                    Y2 = 15.3,
                    Stroke = stroke,
                    StrokeThickness = 1.8
                },
                new Line
                {
                    X1 = 14.6,
                    Y1 = 8.7,
                    X2 = 14.6,
                    Y2 = 15.3,
                    Stroke = stroke,
                    StrokeThickness = 1.8
                },
                new Line
                {
                    X1 = 5.4,
                    Y1 = 15.3,
                    X2 = 14.6,
                    Y2 = 15.3,
                    Stroke = stroke,
                    StrokeThickness = 1.8
                },
                new Line
                {
                    X1 = 10,
                    Y1 = 11.6,
                    X2 = 10,
                    Y2 = 15.3,
                    Stroke = stroke,
                    StrokeThickness = 1.8
                }
            }
        };
    }

    private static View BuildExploreIcon(bool active)
    {
        var strokeColor = active ? Color.FromArgb("#DC2626") : Color.FromArgb("#94A3B8");
        var accentColor = active ? Color.FromArgb("#FB7185") : Color.FromArgb("#CBD5E1");

        return new Grid
        {
            WidthRequest = 20,
            HeightRequest = 20,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Ellipse
                {
                    WidthRequest = 18,
                    HeightRequest = 18,
                    Stroke = new SolidColorBrush(strokeColor),
                    StrokeThickness = 1.7,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                },
                new Polygon
                {
                    Points = new PointCollection
                    {
                        new Point(12, 6),
                        new Point(8.2, 8.2),
                        new Point(6, 12),
                        new Point(9.8, 9.8)
                    },
                    Fill = new SolidColorBrush(accentColor),
                    Stroke = new SolidColorBrush(strokeColor),
                    StrokeThickness = 1.1,
                    WidthRequest = 12,
                    HeightRequest = 12,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                },
                new Ellipse
                {
                    WidthRequest = 3.2,
                    HeightRequest = 3.2,
                    Fill = new SolidColorBrush(strokeColor),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                }
            }
        };
    }
}
