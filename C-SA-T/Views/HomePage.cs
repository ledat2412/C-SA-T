using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Shapes;
using MauiApp1.Controls;
using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.Utils;
using MauiApp1.Views.Maps;

namespace MauiApp1.Views;

public class HomePage : ContentPage
{
    private readonly GianHangService _gianHangService;
    private readonly GeofenceEngineService _geofenceEngine;
    private readonly LocalizationService _loc;
    private static HttpClient? _imageRenderHttpClient;
    private static readonly ConcurrentDictionary<string, byte[]> _imageBytesCache = new();
    private Location? _userLocation;
    private readonly VerticalStackLayout _nearbySection;
    private readonly Label _heroFollowLabel;
    private Label _heroBadgeLabel = null!;
    private Label _heroStreetLabel = null!;
    private Label _sectionNearbyLabel = null!;
    private int _followCount;

    public HomePage(GianHangService gianHangService, GeofenceEngineService geofenceEngine, LocalizationService localizationService)
    {
        _gianHangService = gianHangService;
        _geofenceEngine = geofenceEngine;
        _loc = localizationService;

        Title = string.Empty;
        BackgroundColor = Color.FromArgb("#FFF7F1");

        NavigationPage.SetHasNavigationBar(this, false);

        _nearbySection = new VerticalStackLayout { Spacing = 12 };
        _heroFollowLabel = new Label
        {
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#D6E4FF")
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
                Padding = new Thickness(16, 18, 16, 28),
                Children =
                {
                    BuildMainHeader(),
                    BuildHeroCard(),
                    _nearbySection
                }
            }
        };

        root.Children.Add(scroll);

        var footer = new AppBottomBar(
            BottomBarTab.Home,
            localizationService,
            onExploreTap: async () =>
            {
                var poiMapPage = App.Current?.Handler?.MauiContext?.Services.GetRequiredService<PoiMapPage>();
                if (poiMapPage != null)
                {
                    poiMapPage.RequestAutoOpenExplore();
                    await Navigation.PushAsync(poiMapPage);
                }
            },
            onSettingsTap: async () =>
            {
                var settingsPage = App.Current?.Handler?.MauiContext?.Services.GetRequiredService<SettingsPage>();
                if (settingsPage != null)
                    await Navigation.PushAsync(settingsPage);
            });
        root.Children.Add(footer);
        Grid.SetRow(footer, 1);

        var audioBanner = new AudioPlaybackBanner(_geofenceEngine, localizationService);
        root.Children.Add(audioBanner);
        Grid.SetRowSpan(audioBanner, 2);

        Content = root;

        localizationService.LanguageChanged += OnLanguageChanged;
        UpdateLocalizedText();

        Appearing += async (_, __) => await LoadNearbyRestaurants();
    }

    private void OnLanguageChanged()
    {
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            UpdateLocalizedText();
            await LoadNearbyRestaurants();
        });
    }

    private void UpdateLocalizedText()
    {
        _heroBadgeLabel.Text = _loc.Get("hero_badge");
        _heroStreetLabel.Text = _loc.Get("hero_street");
        _heroFollowLabel.Text = string.Format(_loc.Get("hero_follow"), _followCount);
        if (_sectionNearbyLabel is not null)
            _sectionNearbyLabel.Text = _loc.Get("section_nearby");
    }

    private async Task LoadNearbyRestaurants()
    {
        try
        {
            await GetUserLocation();

            var gianHangs = await _gianHangService.GetAllAsync(_loc.CurrentLanguage);

            _nearbySection.Children.Clear();
            _sectionNearbyLabel = null!;
            _nearbySection.Children.Add(BuildSectionHeader(_loc.Get("section_nearby"), _loc.Get("section_see_all")));

            if (gianHangs == null || gianHangs.Count == 0)
            {
                _nearbySection.Children.Add(new Label
                {
                    Text = _loc.Get("no_data"),
                    FontSize = 14,
                    TextColor = Color.FromArgb("#64748B")
                });
                return;
            }

            var restaurantsWithDistance = new List<(GianHang restaurant, double distance, string imagePath)>();

            foreach (var gh in gianHangs)
            {
                if (gh.Lat == null || gh.Lon == null)
                    continue;

                var userLocation = _userLocation ?? new Location(10.762622, 106.660172);
                var distance = CalculateDistance(
                    userLocation.Latitude,
                    userLocation.Longitude,
                    gh.Lat.Value,
                    gh.Lon.Value);

                var imagePath = !string.IsNullOrWhiteSpace(gh.HinhAnhChinh)
                    ? gh.HinhAnhChinh
                    : gh.HinhAnh;

                restaurantsWithDistance.Add((gh, distance, imagePath ?? "dotnet_bot.png"));
            }

            var sorted = restaurantsWithDistance.OrderBy(r => r.distance).ToList();
            _followCount = Math.Min(sorted.Count, 8);
            _heroFollowLabel.Text = string.Format(_loc.Get("hero_follow"), _followCount);
            await _geofenceEngine.UpdateTargetsAsync(gianHangs, radiusMeters: 10);
            await _geofenceEngine.StartAsync();

            foreach (var (restaurant, distance, imagePath) in sorted.Take(8))
            {
                var distanceText = distance < 1
                    ? $"{distance * 1000:F0} m"
                    : $"{distance:F1} km";

                _nearbySection.Children.Add(BuildNearbySpotRow(
                    restaurant,
                    string.IsNullOrWhiteSpace(restaurant.DiaChi)
                        ? $"G\u1EA7n b\u1EA1n • {distanceText}"
                        : $"{restaurant.DiaChi} • {distanceText}",
                    imagePath));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomePage] Error loading nearby restaurants: {ex.Message}");
        }
    }

    private async Task GetUserLocation()
    {
        try
        {
            var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(30)
            });

            _userLocation = location ?? new Location(10.762622, 106.660172);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[HomePage] Geolocation error: {ex.Message}");
            _userLocation = new Location(10.762622, 106.660172);
        }
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return r * c;
    }

    private View BuildMainHeader()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };

        var left = new VerticalStackLayout
        {
            Spacing = 6,
            Children =
            {
                new Label
                {
                    Text = "Khu ph\u1ED1 V\u0129nh Kh\u00E1nh",
                    FontSize = 32,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#0F172A"),
                    LineHeight = 1.05
                },
                new Border
                {
                    StrokeThickness = 0,
                    BackgroundColor = Color.FromArgb("#FFF0E6"),
                    StrokeShape = new RoundRectangle { CornerRadius = 999 },
                    HorizontalOptions = LayoutOptions.Start,
                    Padding = new Thickness(10, 6),
                    Content = new HorizontalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            new BoxView
                            {
                                WidthRequest = 6,
                                HeightRequest = 6,
                                CornerRadius = 3,
                                Color = Color.FromArgb("#F97316"),
                                VerticalOptions = LayoutOptions.Center
                            },
                            new Label
                            {
                                Text = "Qu\u1EADn 4, TP H\u1ED3 Ch\u00ED Minh",
                                FontSize = 13,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#9A3412"),
                                VerticalTextAlignment = TextAlignment.Center
                            }
                        }
                    }
                }
            }
        };
        grid.Children.Add(left);

        var notifyButton = new Border
        {
            HeightRequest = 44,
            WidthRequest = 44,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 22 },
            BackgroundColor = Colors.White,
            Content = new Grid
            {
                Children =
                {
                    BuildBellIcon(),
                    new Border
                    {
                        WidthRequest = 10,
                        HeightRequest = 10,
                        StrokeThickness = 2,
                        Stroke = new SolidColorBrush(Colors.White),
                        StrokeShape = new RoundRectangle { CornerRadius = 5 },
                        BackgroundColor = Color.FromArgb("#F59E0B"),
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.Start,
                        TranslationX = -2,
                        TranslationY = 2
                    }
                }
            },
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.08f,
                Radius = 10,
                Offset = new Point(0, 4)
            }
        };
        grid.Children.Add(notifyButton);
        Grid.SetColumn(notifyButton, 1);

        return grid;
    }

    private View BuildHeroCard()
    {
        var content = new Grid();

        content.Children.Add(new Border
        {
            StrokeThickness = 0,
            WidthRequest = 126,
            HeightRequest = 126,
            StrokeShape = new RoundRectangle { CornerRadius = 63 },
            BackgroundColor = Color.FromArgb("#14FFFFFF"),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            TranslationX = 34,
            TranslationY = -26
        });

        content.Children.Add(new Border
        {
            StrokeThickness = 0,
            WidthRequest = 94,
            HeightRequest = 94,
            StrokeShape = new RoundRectangle { CornerRadius = 47 },
            BackgroundColor = Color.FromArgb("#24FB7185"),
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.End,
            TranslationX = 18,
            TranslationY = 20
        });

        _heroBadgeLabel = new Label
        {
            FontSize = 13,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FDE68A"),
            VerticalTextAlignment = TextAlignment.Center
        };

        _heroStreetLabel = new Label
        {
            FontSize = 34,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.White
        };

        content.Children.Add(new VerticalStackLayout
        {
            Spacing = 8,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Border
                {
                    StrokeThickness = 0,
                    StrokeShape = new RoundRectangle { CornerRadius = 999 },
                    BackgroundColor = Color.FromArgb("#1FFFFFFF"),
                    HorizontalOptions = LayoutOptions.Start,
                    Padding = new Thickness(10, 6),
                    Content = new HorizontalStackLayout
                    {
                        Spacing = 6,
                        Children =
                        {
                            new BoxView
                            {
                                WidthRequest = 6,
                                HeightRequest = 6,
                                CornerRadius = 3,
                                Color = Color.FromArgb("#FCA5A5"),
                                VerticalOptions = LayoutOptions.Center
                            },
                            _heroBadgeLabel
                        }
                    }
                },
                _heroStreetLabel,
                new Label
                {
                    Text = "V\u0129nh Kh\u00E1nh",
                    FontSize = 34,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#FCA5A5")
                },
                _heroFollowLabel
            }
        });

        return new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Padding = new Thickness(18),
            HeightRequest = 194,
            Background = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#21356B"), 0f),
                    new GradientStop(Color.FromArgb("#16213E"), 0.55f),
                    new GradientStop(Color.FromArgb("#101827"), 1f)
                },
                new Point(0, 0),
                new Point(1, 1)),
            Content = content,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.12f,
                Radius = 18,
                Offset = new Point(0, 10)
            }
        };
    }

    private View BuildNearbySpotRow(GianHang restaurant, string subtitle, string imagePath)
    {
        var rowGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12,
            VerticalOptions = LayoutOptions.Center
        };

        rowGrid.Children.Add(new Border
        {
            StrokeThickness = 0,
            HeightRequest = 58,
            WidthRequest = 58,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Content = new Image
            {
                Source = BuildImageSource(imagePath),
                Aspect = Aspect.AspectFill
            },
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.06f,
                Radius = 8,
                Offset = new Point(0, 3)
            }
        });

        var textWrap = new VerticalStackLayout
        {
            Spacing = 4,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = restaurant.Ten ?? "Quán ăn",
                    FontSize = 16,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#0F172A"),
                    MaxLines = 1,
                    LineBreakMode = LineBreakMode.TailTruncation
                },
                new Label
                {
                    Text = subtitle,
                    FontSize = 12,
                    TextColor = Color.FromArgb("#64748B"),
                    MaxLines = 2,
                    LineBreakMode = LineBreakMode.TailTruncation
                }
            }
        };
        rowGrid.Children.Add(textWrap);
        Grid.SetColumn(textWrap, 1);

        var playButton = new Border
        {
            StrokeThickness = 0,
            HeightRequest = 40,
            WidthRequest = 40,
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            Background = new LinearGradientBrush(
                new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb("#EF4444"), 0f),
                    new GradientStop(Color.FromArgb("#DC2626"), 1f)
                },
                new Point(0, 0),
                new Point(1, 1)),
            VerticalOptions = LayoutOptions.Center,
            Content = BuildPlayIcon(),
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.08f,
                Radius = 10,
                Offset = new Point(0, 4)
            }
        };
        var playTap = new TapGestureRecognizer();
        playTap.Tapped += async (_, __) => await PlayStoreAudioAsync(restaurant);
        playButton.GestureRecognizers.Add(playTap);
        rowGrid.Children.Add(playButton);
        Grid.SetColumn(playButton, 2);

        return new Border
        {
            StrokeThickness = 1,
            Stroke = new SolidColorBrush(Color.FromArgb("#F3E8E2")),
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            Padding = new Thickness(10),
            Content = rowGrid,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.04f,
                Radius = 10,
                Offset = new Point(0, 3)
            }
        };
    }

    private async Task PlayStoreAudioAsync(GianHang restaurant)
    {
        if (string.IsNullOrWhiteSpace(restaurant.AudioFullUrl))
        {
            await DisplayAlertAsync(_loc.Get("alert_notice"), _loc.Get("alert_no_audio"), _loc.Get("alert_ok"));
            return;
        }

        await _geofenceEngine.TogglePlaybackAsync(new AudioPlaybackRequest(
            restaurant.IdGianHang,
            string.IsNullOrWhiteSpace(restaurant.Ten) ? "Quán ăn" : restaurant.Ten,
            restaurant.AudioFullUrl,
            restaurant.HinhAnhFullUrl));
    }

    private static string NormalizeImagePath(string? dbPath)
    {
        if (string.IsNullOrWhiteSpace(dbPath))
            return "dotnet_bot.png";

        dbPath = dbPath.Trim();

        if (dbPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            dbPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            return dbPath;
        }

        var normalizedPath = dbPath.Replace("\\", "/");
        if (!normalizedPath.Contains('/'))
            return normalizedPath;

        return BuildFullUrl(normalizedPath);
    }

    private static bool IsRemoteImageUrl(string value)
    {
        return value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               value.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }

    private static ImageSource BuildImageSource(string? dbPath)
    {
        var normalized = NormalizeImagePath(dbPath);

        if (!IsRemoteImageUrl(normalized))
            return normalized;

        return ImageSource.FromStream(async cancellationToken =>
        {
            try
            {
                if (_imageBytesCache.TryGetValue(normalized, out var cachedBytes))
                    return new MemoryStream(cachedBytes);

                _imageRenderHttpClient ??= CreateImageRenderHttpClient();

                using var response = await _imageRenderHttpClient.GetAsync(normalized, cancellationToken)
                    .ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return await FileSystem.OpenAppPackageFileAsync("dotnet_bot.png");

                var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                _imageBytesCache[normalized] = bytes;
                return new MemoryStream(bytes);
            }
            catch
            {
                return await FileSystem.OpenAppPackageFileAsync("dotnet_bot.png");
            }
        });
    }

    private static string BuildFullUrl(string path)
    {
        return BackendUrlResolver.BuildUrl(path);
    }

    private static HttpClient CreateImageRenderHttpClient()
    {
        var handler = new HttpClientHandler();

#if DEBUG
        handler.ServerCertificateCustomValidationCallback =
            (_, _, _, _) => true;
#endif

        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
    }

    private View BuildBottomPlayerStub()
    {
        var cover = new Border
        {
            StrokeThickness = 0,
            HeightRequest = 42,
            WidthRequest = 42,
            StrokeShape = new RoundRectangle { CornerRadius = 12 },
            Content = new Image
            {
                Source = "dotnet_bot.png",
                Aspect = Aspect.AspectFill
            }
        };

        var textWrap = new VerticalStackLayout
        {
            Spacing = 0,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Label
                {
                    Text = "\u0110ang ph\u00E1t",
                    FontSize = 11,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#9A3412")
                },
                new Label
                {
                    Text = "X\u00F3m Chi\u1EBFu - A Century of Mat-Weaving",
                    FontSize = 13,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.FromArgb("#0F172A"),
                    MaxLines = 1,
                    LineBreakMode = LineBreakMode.TailTruncation
                }
            }
        };

        var closeButton = new Border
        {
            StrokeThickness = 0,
            HeightRequest = 30,
            WidthRequest = 30,
            StrokeShape = new RoundRectangle { CornerRadius = 15 },
            BackgroundColor = Colors.White,
            VerticalOptions = LayoutOptions.Center,
            Content = BuildCloseIcon(),
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.05f,
                Radius = 8,
                Offset = new Point(0, 2)
            }
        };

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 10
        };
        content.Children.Add(cover);
        content.Children.Add(textWrap);
        content.Children.Add(closeButton);
        Grid.SetColumn(textWrap, 1);
        Grid.SetColumn(closeButton, 2);

        return new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            BackgroundColor = Color.FromArgb("#FFF6EF"),
            Padding = new Thickness(12, 10),
            Content = content,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.04f,
                Radius = 12,
                Offset = new Point(0, 3)
            }
        };
    }

    private View BuildSectionHeader(string title, string action)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        _sectionNearbyLabel = new Label
        {
            Text = title,
            FontSize = 22,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#0F172A")
        };

        grid.Children.Add(_sectionNearbyLabel);

        var actionLabel = new Label
        {
            Text = action,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#E11D48"),
            VerticalTextAlignment = TextAlignment.End
        };
        grid.Children.Add(actionLabel);
        Grid.SetColumn(actionLabel, 1);

        return grid;
    }

    private View BuildBellIcon()
    {
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
                    Stroke = new SolidColorBrush(Color.FromArgb("#0F172A")),
                    StrokeThickness = 1.6,
                    WidthRequest = 15,
                    HeightRequest = 15,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                },
                new Line
                {
                    X1 = 10,
                    X2 = 10,
                    Y1 = 4.6,
                    Y2 = 10.2,
                    Stroke = new SolidColorBrush(Color.FromArgb("#0F172A")),
                    StrokeThickness = 1.6,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center
                },
                new Ellipse
                {
                    WidthRequest = 3.4,
                    HeightRequest = 3.4,
                    Fill = new SolidColorBrush(Color.FromArgb("#0F172A")),
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.End,
                    TranslationY = -1
                }
            }
        };
    }

    private View BuildPlayIcon()
    {
        return new Polygon
        {
            Points = new PointCollection
            {
                new Point(6, 4.5),
                new Point(6, 15.5),
                new Point(15, 10)
            },
            Fill = new SolidColorBrush(Colors.White),
            StrokeThickness = 0,
            WidthRequest = 18,
            HeightRequest = 18,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };
    }

    private View BuildCloseIcon()
    {
        var stroke = new SolidColorBrush(Color.FromArgb("#64748B"));

        return new Grid
        {
            WidthRequest = 14,
            HeightRequest = 14,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Children =
            {
                new Line
                {
                    X1 = 3,
                    Y1 = 3,
                    X2 = 11,
                    Y2 = 11,
                    Stroke = stroke,
                    StrokeThickness = 1.8
                },
                new Line
                {
                    X1 = 11,
                    Y1 = 3,
                    X2 = 3,
                    Y2 = 11,
                    Stroke = stroke,
                    StrokeThickness = 1.8
                }
            }
        };
    }

}
