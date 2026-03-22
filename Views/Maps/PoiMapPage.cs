using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Maps;
using Map = Microsoft.Maui.Controls.Maps.Map;

namespace MauiApp1.Views.Maps;

public class PoiMapPage : ContentPage
{
    readonly Map _map;

    readonly Grid _bottomSheet;
    readonly ScrollView _poiScroll;
    readonly VerticalStackLayout _poiList;

    readonly Label _titleLabel;
    readonly Label _subtitleLabel;

    readonly List<PoiItem> _pois;

    double _sheetHiddenY;   // Ẩn hoàn toàn, nằm dưới màn hình
    double _sheetMiniY;     // Mốc thấp
    double _sheetHalfY;     // Mốc giữa
    double _sheetFullY;     // Mốc cao

    double _currentSheetY;
    double _panStartSheetY;

    bool _isLayoutReady;
    bool _isAnimating;
    bool _isExploreVisible;

    public PoiMapPage()
    {
        Title = "Khám phá";
        BackgroundColor = Colors.White;

        _pois = CreateDemoPois();

        _map = CreateMap();

        _titleLabel = new Label
        {
            Text = "Khám phá",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#111111")
        };

        _subtitleLabel = new Label
        {
            Text = "Ẩm thực, đồ uống và các địa điểm gần bạn",
            FontSize = 13,
            TextColor = Color.FromArgb("#6B7280")
        };

        _poiList = new VerticalStackLayout
        {
            Spacing = 12,
            Padding = new Thickness(16, 0, 16, 24)
        };

        foreach (var poi in _pois)
            _poiList.Children.Add(CreatePoiCard(poi));

        _poiScroll = new ScrollView
        {
            Content = _poiList
        };

        _bottomSheet = CreateBottomSheet();

        Content = BuildLayout();

        Loaded += (_, __) => InitializeSheetPositions();
        SizeChanged += (_, __) => InitializeSheetPositions();

        AddPinsToMap();
    }

    View BuildLayout()
    {
        var root = new Grid();

        root.Children.Add(_map);
        root.Children.Add(_bottomSheet);
        root.Children.Add(BuildFooter());

        return root;
    }

    Map CreateMap()
    {
        var center = new Location(10.762622, 106.660172);

        return new Map(MapSpan.FromCenterAndRadius(center, Distance.FromKilometers(1)))
        {
            IsShowingUser = false,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill
        };
    }

    void AddPinsToMap()
    {
        _map.Pins.Clear();

        foreach (var poi in _pois)
        {
            _map.Pins.Add(new Pin
            {
                Label = poi.Title,
                Address = poi.Subtitle,
                Type = PinType.Place,
                Location = new Location(poi.Latitude, poi.Longitude)
            });
        }
    }

    Grid CreateBottomSheet()
    {
        var dragBar = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#D4D4D8"),
            StrokeShape = new RoundRectangle { CornerRadius = 999 },
            HeightRequest = 5,
            WidthRequest = 48,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10, 0, 10)
        };

        var dragArea = new Grid
        {
            HeightRequest = 34,
            Children = { dragBar }
        };

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnSheetPanUpdated;
        dragArea.GestureRecognizers.Add(pan);

        var nearBadge = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#FFE8D6"),
            StrokeShape = new RoundRectangle { CornerRadius = 999 },
            Padding = new Thickness(12, 6),
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = "Gần bạn",
                FontSize = 12,
                TextColor = Color.FromArgb("#9A3412")
            }
        };

        var header = new VerticalStackLayout
        {
            Spacing = 8,
            Padding = new Thickness(18, 0, 18, 12),
            Children =
            {
                _titleLabel,
                _subtitleLabel,
                nearBadge
            }
        };

        var body = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };

        body.Children.Add(dragArea);

        body.Children.Add(header);
        Grid.SetRow(header, 1);

        body.Children.Add(_poiScroll);
        Grid.SetRow(_poiScroll, 2);

        var panel = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#FFF8F1"),
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(28, 28, 0, 0)
            },
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.12f,
                Radius = 18,
                Offset = new Point(0, -4)
            },
            Content = body,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill
        };

        return new Grid
        {
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            Children = { panel },
            IsVisible = false // ban đầu tắt hẳn
        };
    }

    View BuildFooter()
    {
        View BuildFooterItem(string emoji, string text, bool active, Func<Task>? onTap = null)
        {
            var iconWrap = new Border
            {
                StrokeThickness = 0,
                BackgroundColor = active ? Color.FromArgb("#F4E3D7") : Colors.Transparent,
                StrokeShape = new RoundRectangle { CornerRadius = 16 },
                Padding = new Thickness(10, 6),
                HorizontalOptions = LayoutOptions.Center,
                Content = new Label
                {
                    Text = emoji,
                    FontSize = 20,
                    HorizontalTextAlignment = TextAlignment.Center
                }
            };

            var label = new Label
            {
                Text = text,
                FontSize = 12,
                TextColor = active ? Color.FromArgb("#111111") : Color.FromArgb("#8A8A8A"),
                HorizontalTextAlignment = TextAlignment.Center
            };

            var stack = new VerticalStackLayout
            {
                Spacing = 4,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                Children = { iconWrap, label }
            };

            if (onTap is not null)
            {
                var tap = new TapGestureRecognizer();
                tap.Tapped += async (_, __) => await onTap();
                stack.GestureRecognizers.Add(tap);
            }

            return stack;
        }

        var home = BuildFooterItem("🏠", "Trang chủ", false);
        var explore = BuildFooterItem("🧭", "Khám phá", true, ToggleSuggestionSheetAsync);
        var map = BuildFooterItem("🗺️", "Bản đồ", false);
        var profile = BuildFooterItem("👤", "Tôi", false);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            Padding = new Thickness(16, 10)
        };

        grid.Children.Add(home);

        grid.Children.Add(explore);
        Grid.SetColumn(explore, 1);

        grid.Children.Add(map);
        Grid.SetColumn(map, 2);

        grid.Children.Add(profile);
        Grid.SetColumn(profile, 3);

        return new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = 28 },
            Content = grid,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.14f,
                Radius = 18,
                Offset = new Point(0, 6)
            },
            Margin = new Thickness(14, 0, 14, 12),
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Fill
        };
    }

    async Task ToggleSuggestionSheetAsync()
    {
        if (!_isLayoutReady || _isAnimating)
            return;

        if (!_isExploreVisible)
        {
            _bottomSheet.IsVisible = true;
            _bottomSheet.TranslationY = _sheetHiddenY;
            _currentSheetY = _sheetHiddenY;
            _isExploreVisible = true;

            await SnapSheetToAsync(_sheetHalfY);
            return;
        }

        await HideSheetAsync();
    }

    async Task HideSheetAsync()
    {
        if (!_isExploreVisible || _isAnimating)
            return;

        _isAnimating = true;

        try
        {
            await _bottomSheet.TranslateTo(0, _sheetHiddenY, 180, Easing.CubicIn);

            _currentSheetY = _sheetHiddenY;
            _bottomSheet.TranslationY = _sheetHiddenY;
            _bottomSheet.IsVisible = false;
            _isExploreVisible = false;

            UpdateScrollAvailability();
            UpdateHeaderByState();
        }
        finally
        {
            _isAnimating = false;
        }
    }

    void InitializeSheetPositions()
    {
        if (Width <= 0 || Height <= 0)
            return;

        _sheetFullY = Height * 0.12;
        _sheetHalfY = Height * 0.46;
        _sheetMiniY = Height * 0.72;
        _sheetHiddenY = Height + 20;

        if (!_isLayoutReady)
        {
            _currentSheetY = _sheetHiddenY;
            _bottomSheet.TranslationY = _sheetHiddenY;
            _bottomSheet.IsVisible = false;
            _isExploreVisible = false;

            UpdateScrollAvailability();
            UpdateHeaderByState();
            _isLayoutReady = true;
        }
    }

    void OnSheetPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (!_isLayoutReady || _isAnimating || !_isExploreVisible)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panStartSheetY = _currentSheetY;
                break;

            case GestureStatus.Running:
                var nextY = _panStartSheetY + e.TotalY;

                // Chỉ cho kéo trong vùng đang hiển thị, không cho kéo tuột xuống để ẩn
                nextY = Math.Max(_sheetFullY, Math.Min(_sheetMiniY, nextY));

                _currentSheetY = nextY;
                _bottomSheet.TranslationY = _currentSheetY;

                UpdateScrollAvailability();
                break;

            case GestureStatus.Canceled:
            case GestureStatus.Completed:
                _ = SnapSheetToAsync(ResolveSnapTarget(_currentSheetY));
                break;
        }
    }

    double ResolveSnapTarget(double y)
    {
        var points = new[] { _sheetFullY, _sheetHalfY, _sheetMiniY };
        return points.OrderBy(p => Math.Abs(p - y)).First();
    }

    async Task SnapSheetToAsync(double targetY)
    {
        if (_isAnimating)
            return;

        _isAnimating = true;

        try
        {
            targetY = Math.Max(_sheetFullY, Math.Min(_sheetMiniY, targetY));

            await _bottomSheet.TranslateTo(0, targetY, 180, Easing.CubicOut);

            _currentSheetY = targetY;
            _bottomSheet.TranslationY = targetY;

            UpdateScrollAvailability();
            UpdateHeaderByState();
        }
        finally
        {
            _isAnimating = false;
        }
    }

    void UpdateScrollAvailability()
    {
        if (!_isExploreVisible)
        {
            _poiScroll.InputTransparent = true;
            return;
        }

        var enableScroll = _currentSheetY <= _sheetHalfY + 4;
        _poiScroll.InputTransparent = !enableScroll;
    }

    void UpdateHeaderByState()
    {
        if (!_isExploreVisible)
        {
            _subtitleLabel.Text = "Ẩm thực, đồ uống và các địa điểm gần bạn";
            return;
        }

        if (_currentSheetY <= _sheetFullY + 10)
        {
            _subtitleLabel.Text = "Danh sách đầy đủ các địa điểm gợi ý";
        }
        else if (_currentSheetY <= _sheetHalfY + 10)
        {
            _subtitleLabel.Text = "Chạm vào quán để đưa map đến vị trí";
        }
        else
        {
            _subtitleLabel.Text = "Ẩm thực, đồ uống và các địa điểm gần bạn";
        }
    }

    View CreatePoiCard(PoiItem poi)
    {
        var image = new Image
        {
            Source = string.IsNullOrWhiteSpace(poi.ImagePath) ? "dotnet_bot.png" : poi.ImagePath,
            HeightRequest = 76,
            WidthRequest = 76,
            Aspect = Aspect.AspectFill
        };

        var imageWrap = new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            HeightRequest = 76,
            WidthRequest = 76,
            Content = image
        };

        var title = new Label
        {
            Text = poi.Title,
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#111111"),
            LineBreakMode = LineBreakMode.TailTruncation
        };

        var subtitle = new Label
        {
            Text = poi.Subtitle,
            FontSize = 13,
            TextColor = Color.FromArgb("#6B7280"),
            LineBreakMode = LineBreakMode.WordWrap
        };

        var tag = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#FFF1E6"),
            StrokeShape = new RoundRectangle { CornerRadius = 999 },
            Padding = new Thickness(10, 4),
            HorizontalOptions = LayoutOptions.Start,
            Content = new Label
            {
                Text = "Xem chi tiết",
                FontSize = 11,
                TextColor = Color.FromArgb("#9A3412")
            }
        };

        var textArea = new VerticalStackLayout
        {
            Spacing = 6,
            VerticalOptions = LayoutOptions.Center,
            Children = { title, subtitle, tag }
        };

        var content = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 12
        };

        content.Children.Add(imageWrap);
        content.Children.Add(textArea);
        Grid.SetColumn(textArea, 1);

        var card = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#F8EDE3"),
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Padding = new Thickness(12),
            Content = content
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, __) => await FocusPoiAsync(poi);
        card.GestureRecognizers.Add(tap);

        return card;
    }

    async Task FocusPoiAsync(PoiItem poi)
    {
        var location = new Location(poi.Latitude, poi.Longitude);
        _map.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromMeters(250)));

        _titleLabel.Text = poi.Title;
        _subtitleLabel.Text = poi.Subtitle;

        await Task.CompletedTask;
    }

    List<PoiItem> CreateDemoPois()
    {
        return new List<PoiItem>
        {
            new PoiItem
            {
                Title = "Bánh mì Minh Nhật",
                Subtitle = "Bánh mì chảo, thịt nướng, pate nhà làm",
                ImagePath = "food1.jpg",
                Latitude = 10.762750,
                Longitude = 106.659850
            },
            new PoiItem
            {
                Title = "Trà sữa Phố Ngọt",
                Subtitle = "Hồng trà, trà sữa, topping tự chọn",
                ImagePath = "food2.jpg",
                Latitude = 10.763180,
                Longitude = 106.660650
            },
            new PoiItem
            {
                Title = "Cơm tấm Sài Gòn",
                Subtitle = "Sườn bì chả, món trưa bình dân",
                ImagePath = "food3.jpg",
                Latitude = 10.761980,
                Longitude = 106.661050
            },
            new PoiItem
            {
                Title = "Bún bò Huế Mệ An",
                Subtitle = "Chả cua, bò tái, nước dùng đậm vị",
                ImagePath = "dotnet_bot.png",
                Latitude = 10.762210,
                Longitude = 106.658980
            }
        };
    }

    public class PoiItem
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string ImagePath { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}