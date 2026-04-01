using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Maps;
#if ANDROID
using MauiApp1.Platforms.Android.Maps;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
#endif
using iOSPage = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page;
using Map = Microsoft.Maui.Controls.Maps.Map;
using Path = System.IO.Path;

namespace MauiApp1.Views.Maps;

public class PoiMapPage : ContentPage
{
    readonly Map _map;

    // Sheet khám phá
    private readonly Grid _bottomSheet;
    private readonly ScrollView _poiScroll;
    private readonly VerticalStackLayout _poiList;

    private readonly Label _titleLabel;
    private readonly Label _subtitleLabel;

    readonly List<PoiItem> _pois;

    double _sheetHiddenY;   // Ẩn hoàn toàn, nằm dưới màn hình
    double _sheetMiniY;     // Mốc thấp
    double _sheetHalfY;     // Mốc giữa
    double _sheetFullY;     // Mốc cao

    private double _detailCurrentY;
    private double _detailPanStartY;
    private double _detailLastPanTotalY;
    private double _detailLastPanDeltaY;

    private bool _isDetailVisible;
    private bool _isDetailAnimating;
    private bool _isOpeningFoodGallery;
    private GianHang? _currentDetailGianHang;

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

        _poiScroll = new ScrollView
        {
            Content = _poiList
        };

        _bottomSheet = CreateBottomSheet();
        _detailSheet = CreateDetailSheet();
        _footer = BuildFooter();

        Content = BuildLayout();

        Loaded += async (_, __) =>
        {
            InitializeSheetPositions();
            await LoadRealPoisAsync();
        };

        SizeChanged += (_, __) => InitializeSheetPositions();
    }

    private View BuildLayout()
    {
        var root = new Grid();

        root.Children.Add(_map);
        root.Children.Add(_bottomSheet);
        root.Children.Add(_footer);
        root.Children.Add(_detailSheet);

        return root;
    }

    private Map CreateMap()
    {
        var center = new Location(10.762622, 106.660172);

        return new Map(MapSpan.FromCenterAndRadius(center, Distance.FromKilometers(1)))
        {
            MapType = MapType.Street,
            IsShowingUser = false,
            VerticalOptions = LayoutOptions.Fill,
            HorizontalOptions = LayoutOptions.Fill,
            BackgroundColor = Colors.Transparent
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

        var home = BuildFooterItem("🏠", "Trang chủ", false, async () => await Navigation.PushAsync(new HomePage()));
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
            Padding = new Thickness(20, 12)
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
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(28, 28, 0, 0) },
            Content = grid,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.14f,
                Radius = 18,
                Offset = new Point(0, 6)
            },
            Margin = new Thickness(0, 0, 0, 0),
            VerticalOptions = LayoutOptions.End,
            HorizontalOptions = LayoutOptions.Fill
        };
    }

    private async Task ToggleSuggestionSheetAsync()
    {
        if (!_isLayoutReady || _isAnimating)
            return;

        // Nếu sheet chi tiết đang mở thì đóng nó trước
        if (_isDetailVisible)
        {
            await HideDetailSheetAsync();
        }

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

    private async Task HideSheetAsync()
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

    private async Task ShowDetailSheetAsync(GianHang gianHang)
    {
        if (_isExploreVisible)
        {
            await HideSheetAsync();
        }

        _currentDetailGianHang = gianHang;

        _detailTitle.Text = string.IsNullOrWhiteSpace(gianHang.Ten) ? "Tên gian hàng" : gianHang.Ten;
        _detailAddress.Text = string.IsNullOrWhiteSpace(gianHang.DiaChi) ? "Chưa có địa chỉ" : gianHang.DiaChi;
        _detailDescription.Text = string.IsNullOrWhiteSpace(gianHang.MoTa) ? "Chưa có mô tả." : gianHang.MoTa;

        // Nếu bạn đã có AudioUrl trong model thì thay tại đây
        _detailAudioLabel.Text = "Audio thuyết minh đã sẵn sàng";

        _detailImage.Source = NormalizeImagePath(gianHang.HinhAnh);

        _detailSheet.IsVisible = true;
        _detailSheet.TranslationY = _detailHiddenY;
        _detailCurrentY = _detailHiddenY;
        _isDetailVisible = true;

        await SnapDetailSheetToAsync(_detailHalfY);
    }

    private async Task HideDetailSheetAsync()
    {
        if (!_isDetailVisible || _isDetailAnimating)
            return;

        _isDetailAnimating = true;

        try
        {
            await _detailSheet.TranslateTo(0, _detailHiddenY, 180, Easing.CubicIn);

            _detailCurrentY = _detailHiddenY;
            _detailSheet.TranslationY = _detailHiddenY;
            _detailSheet.IsVisible = false;
            _isDetailVisible = false;
        }
        finally
        {
            _isDetailAnimating = false;
        }
    }

    private void InitializeSheetPositions()
    {
        if (Width <= 0 || Height <= 0)
            return;

        _sheetFullY = Height * 0.12;
        _sheetHalfY = Height * 0.46;

        const double miniPeekHeight = 180;
        _sheetMiniY = Math.Max(_sheetHalfY + 40, Height - miniPeekHeight);

        _sheetHiddenY = Height + 20;

        _detailFullY = Height * 0.02;
        _detailHalfY = Height * 0.22;
        _detailMiniY = Height * 0.58;
        _detailHiddenY = Height + 20;

        if (!_isLayoutReady)
        {
            _currentSheetY = _sheetHiddenY;
            _bottomSheet.TranslationY = _sheetHiddenY;
            _bottomSheet.IsVisible = false;
            _isExploreVisible = false;

            _detailCurrentY = _detailHiddenY;
            _detailSheet.TranslationY = _detailHiddenY;
            _detailSheet.IsVisible = false;
            _isDetailVisible = false;

            UpdateScrollAvailability();
            UpdateHeaderByState();
            _isLayoutReady = true;
        }
    }

    private void OnSheetPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (!_isLayoutReady || _isAnimating || !_isExploreVisible)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _panStartSheetY = _currentSheetY;
                _lastPanTotalY = 0;
                _lastPanDeltaY = 0;
                break;

            case GestureStatus.Running:
                _lastPanDeltaY = e.TotalY - _lastPanTotalY;
                _lastPanTotalY = e.TotalY;

                var nextY = _panStartSheetY + e.TotalY;

                // Chỉ cho kéo trong vùng đang hiển thị, không cho kéo tuột xuống để ẩn
                nextY = Math.Max(_sheetFullY, Math.Min(_sheetMiniY, nextY));

                _currentSheetY = nextY;
                _bottomSheet.TranslationY = _currentSheetY;

                UpdateScrollAvailability();
                break;

            case GestureStatus.Canceled:
            case GestureStatus.Completed:
                var target = ResolveSnapTargetWithDirection(_currentSheetY, _lastPanDeltaY);
                if (target >= _sheetHiddenY - 1)
                {
                    _ = HideSheetAsync();
                }
                else
                {
                    _ = SnapSheetToAsync(target);
                }
                break;
        }
    }

    private double ResolveSnapTargetWithDirection(double y, double deltaY)
    {
        var points = new[] { _sheetFullY, _sheetHalfY, _sheetMiniY }.OrderBy(p => p).ToArray();

        if (deltaY > 0 && y >= _sheetMiniY + 24)
            return _sheetHiddenY;

        if (Math.Abs(deltaY) < 1.2)
            return ResolveSnapTarget(y);

        if (deltaY < 0)
        {
            for (var i = points.Length - 1; i >= 0; i--)
            {
                if (points[i] < y)
                    return points[i];
            }

            return points[0];
        }

        for (var i = 0; i < points.Length; i++)
        {
            if (points[i] > y)
                return points[i];
        }

        return points[^1];
    }

    private double ResolveDetailSnapTargetWithDirection(double y, double deltaY)
    {
        var points = new[] { _detailFullY, _detailHalfY, _detailMiniY }.OrderBy(p => p).ToArray();

        if (y >= _detailMiniY + 8)
            return _detailHiddenY;

        if (deltaY > 0 && y >= _detailMiniY - 4)
            return _detailHiddenY;

        if (Math.Abs(deltaY) < 1.2)
            return ResolveDetailSnapTarget(y);

        if (deltaY < 0)
        {
            for (var i = points.Length - 1; i >= 0; i--)
            {
                if (points[i] < y)
                    return points[i];
            }

            return points[0];
        }

        for (var i = 0; i < points.Length; i++)
        {
            if (points[i] > y)
                return points[i];
        }

        return points[^1];
    }

    private double ResolveSnapTarget(double y)
    {
        var points = new[] { _sheetFullY, _sheetHalfY, _sheetMiniY };
        return points.OrderBy(p => Math.Abs(p - y)).First();
    }

    private double ResolveDetailSnapTarget(double y)
    {
        var points = new[] { _detailFullY, _detailHalfY, _detailMiniY };
        return points.OrderBy(p => Math.Abs(p - y)).First();
    }

    private void OnDetailPanUpdated(object? sender, PanUpdatedEventArgs e)
    {
        if (!_isLayoutReady || _isDetailAnimating || !_isDetailVisible)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                _detailPanStartY = _detailCurrentY;
                _detailLastPanTotalY = 0;
                _detailLastPanDeltaY = 0;
                break;

            case GestureStatus.Running:
                _detailLastPanDeltaY = e.TotalY - _detailLastPanTotalY;
                _detailLastPanTotalY = e.TotalY;

                var nextY = _detailPanStartY + e.TotalY;
                nextY = Math.Max(_detailFullY, Math.Min(_detailHiddenY, nextY));

                _detailCurrentY = nextY;
                _detailSheet.TranslationY = _detailCurrentY;
                break;

            case GestureStatus.Canceled:
            case GestureStatus.Completed:
                var target = ResolveDetailSnapTargetWithDirection(_detailCurrentY, _detailLastPanDeltaY);
                if (target >= _detailHiddenY - 1)
                {
                    _ = HideDetailSheetAsync();
                }
                else if (target <= _detailFullY + 2 && _detailLastPanDeltaY < -0.6)
                {
                    _ = OpenFoodGalleryFromDetailAsync();
                }
                else
                {
                    _ = SnapDetailSheetToAsync(target);
                }
                break;
        }
    }

    private async Task OpenFoodGalleryFromDetailAsync()
    {
        if (_isOpeningFoodGallery || _currentDetailGianHang is null)
        {
            await SnapDetailSheetToAsync(_detailFullY);
            return;
        }

        _isOpeningFoodGallery = true;

        try
        {
            await HideDetailSheetAsync();

            var page = new GianHangFoodGalleryPage(
                _currentDetailGianHang,
                _monAnService,
                NormalizeImagePath(_currentDetailGianHang.HinhAnh));

            await Navigation.PushAsync(page);
        }
        finally
        {
            _isOpeningFoodGallery = false;
        }
    }

    private async Task SnapSheetToAsync(double targetY)
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

    private async Task SnapDetailSheetToAsync(double targetY)
    {
        if (_isDetailAnimating)
            return;

        _isDetailAnimating = true;

        try
        {
            targetY = Math.Max(_detailFullY, Math.Min(_detailMiniY, targetY));

            await _detailSheet.TranslateTo(0, targetY, 180, Easing.CubicOut);

            _detailCurrentY = targetY;
            _detailSheet.TranslationY = targetY;
        }
        finally
        {
            _isDetailAnimating = false;
        }
    }

    private void UpdateScrollAvailability()
    {
        if (!_isExploreVisible)
        {
            _poiScroll.InputTransparent = true;
            return;
        }

        var enableScroll = _currentSheetY <= _sheetHalfY + 4;
        _poiScroll.InputTransparent = !enableScroll;
    }

    private void UpdateHeaderByState()
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
            _subtitleLabel.Text = "Chạm vào quán để xem chi tiết";
        }
        else
        {
            _subtitleLabel.Text = "Ẩm thực, đồ uống và các địa điểm gần bạn";
        }
    }

    private View CreatePoiCard(PoiItem poi)
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
        tap.Tapped += async (_, __) => await OpenDetailAsync(poi);
        card.GestureRecognizers.Add(tap);

        return card;
    }

    private async Task OpenDetailAsync(PoiItem poi)
    {
        try
        {
            var allGianHang = await _gianHangService.GetAllAsync();

            var gianHang = allGianHang.FirstOrDefault(x =>
                !string.IsNullOrWhiteSpace(x.Ten) &&
                x.Ten.Trim().Equals(poi.Title?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (gianHang == null)
            {
                await DisplayAlertAsync("Thông báo", "Không tìm thấy gian hàng tương ứng.", "OK");
                return;
            }

            // Đưa map tới vị trí quán trước khi mở sheet
            await FocusPoiAsync(poi);

            // Không PushAsync nữa, chỉ mở sheet chi tiết nổi trên map
            await ShowDetailSheetAsync(gianHang);
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi", ex.Message, "OK");
        }
    }

    private async Task FocusPoiAsync(PoiItem poi)
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