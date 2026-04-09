using MauiApp1.Models;
using MauiApp1.Services;
using MauiApp1.Views;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Maps;
using Microsoft.Maui.Networking;
using Plugin.Maui.Audio;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net;
using System.Security.Cryptography;
using System.Text;
#if ANDROID
using MauiApp1.Platforms.Android.Maps;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Microsoft.Maui.Maps.Handlers;
#endif
using Map = Microsoft.Maui.Controls.Maps.Map;
using Path = System.IO.Path;

namespace MauiApp1.Views.Maps;

public partial class PoiMapPage : ContentPage
{
    private readonly GianHangService _gianHangService;
    private readonly PoiService _poiService;
    private readonly MonAnService _monAnService;
    private readonly GeofenceEngineService _geofenceEngine;
    private readonly SQLiteService _sqliteService;
    private readonly Map _map;
    private readonly View _footer;
    private readonly Grid _topBar;

    private const string DefaultLanguageCode = "vi";

    private Entry _searchEntry = null!;
    private Button _refreshTopButton = null!;
    private Button _myLocationButton = null!;
    private Button _teleportButton = null!;

    // Sheet khám phá
    private readonly Grid _bottomSheet;
    private readonly ScrollView _poiScroll;
    private readonly VerticalStackLayout _poiList;

    private readonly Label _titleLabel;
    private readonly Label _subtitleLabel;

    // Sheet chi tiết
    private readonly Grid _detailSheet;
    private Border _detailPanel = null!;
    private Image _detailImage = null!;
    private Label _detailTitle = null!;
    private Label _detailAddress = null!;
    private Label _detailDescription = null!;
    private Label _detailAudioLabel = null!;
    private HorizontalStackLayout _languageRow = null!;

    // Audio controls
    private Button _playButton = null!;
    private Slider _progressSlider = null!;
    private Label _currentTimeLabel = null!;
    private Label _durationLabel = null!;

    private readonly IAudioManager _audioManager;
    private static HttpClient? _imageProbeHttpClient;
    private static HttpClient? _imageRenderHttpClient;
    private static readonly ConcurrentDictionary<string, byte[]> _imageBytesCache = new();
    private static readonly ConcurrentDictionary<string, byte[]> _audioBytesCache = new();
    private IAudioPlayer? _player;
    private MemoryStream? _audioStream;
    private string? _loadedAudioUrl;
    private System.Timers.Timer? _progressTimer;
    private bool _isDraggingSlider;
    private UserLocationPin? _userLocationPin;
    private bool _geofenceInitialized;

    private readonly List<PoiItem> _pois = new();
    private readonly Dictionary<int, StyledPin> _pinsByPoiId = new();
    private CancellationTokenSource? _pinRefreshCts;

    // Vị trí sheet khám phá
    private double _sheetHiddenY;
    private double _sheetMiniY;
    private double _sheetHalfY;
    private double _sheetFullY;

    private double _currentSheetY;
    private double _panStartSheetY;
    private double _lastPanTotalY;
    private double _lastPanDeltaY;

    private bool _isLayoutReady;
    private bool _isAnimating;
    private bool _isExploreVisible;
    private bool _shouldAutoOpenExplore;
    private bool _isInitialLoadStarted;
    private bool _isInitialLoadCompleted;
    private bool _isLoadingPois;

    // Vị trí sheet chi tiết
    private double _detailHiddenY;
    private double _detailMiniY;
    private double _detailHalfY;
    private double _detailFullY;

    private double _detailCurrentY;
    private double _detailPanStartY;
    private double _detailLastPanTotalY;
    private double _detailLastPanDeltaY;

    private bool _isDetailVisible;
    private bool _isDetailAnimating;
    private bool _isOpeningFoodGallery;
    private GianHang? _currentDetailGianHang;
    private readonly List<NgonNgu> _languages = new();
    private string _selectedLanguageCode = DefaultLanguageCode;

#if ANDROID
    private GoogleMap? _androidGoogleMap;
#endif

    public PoiMapPage(
        PoiService poiService,
        GianHangService gianHangService,
        MonAnService monAnService,
        GeofenceEngineService geofenceEngine,
        SQLiteService sqliteService)
    {
        _poiService = poiService;
        _gianHangService = gianHangService;
        _monAnService = monAnService;
        _geofenceEngine = geofenceEngine;
        _sqliteService = sqliteService;
        _audioManager = AudioManager.Current;

        Title = "";
        BackgroundColor = Colors.White;
        SafeAreaEdges = SafeAreaEdges.None;

        Microsoft.Maui.Controls.NavigationPage.SetHasNavigationBar(this, false);

        _map = CreateMap();
        _map.PropertyChanged += OnMapPropertyChanged;

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
        _topBar = CreateTopBar();

        Content = BuildLayout();

#if ANDROID
        _map.HandlerChanged += (_, __) => TryInitializeAndroidMap();
#endif

        Loaded += (_, __) =>
        {
            InitializeSheetPositions();

#if ANDROID
            TryInitializeAndroidMap();
            UpdateAndroidMapPadding();
#endif

            if (_isInitialLoadStarted)
                return;

            _isInitialLoadStarted = true;
            _ = InitializePageAsync();
        };

        Appearing += async (_, __) =>
        {
            await EnsureExploreSheetVisibleAsync();
            await ShowCurrentLocationMarkerAsync(centerOnUser: false);
            await InitializeGeofenceAsync();
            await _geofenceEngine.StartAsync();
        };
        SizeChanged += (_, __) => InitializeSheetPositions();
    }

    private View BuildLayout()
    {
        var root = new Grid();

        root.Children.Add(_map);
        root.Children.Add(_topBar);
        root.Children.Add(_bottomSheet);
        root.Children.Add(_detailSheet);
        root.Children.Add(_footer);

        return root;
    }

    private Grid CreateTopBar()
    {
        _searchEntry = new Entry
        {
            Placeholder = "Tìm kiếm gian hàng",
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb("#111111"),
            PlaceholderColor = Color.FromArgb("#9CA3AF"),
            FontSize = 15,
            ClearButtonVisibility = ClearButtonVisibility.WhileEditing,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center
        };

        var searchIcon = new Label
        {
            Text = "🔍",
            FontSize = 18,
            TextColor = Color.FromArgb("#6B7280"),
            VerticalTextAlignment = TextAlignment.Center,
            HorizontalTextAlignment = TextAlignment.Center,
            WidthRequest = 28
        };

        var searchGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 8,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill
        };

        searchGrid.Children.Add(_searchEntry);
        Grid.SetColumn(_searchEntry, 0);

        searchGrid.Children.Add(searchIcon);
        Grid.SetColumn(searchIcon, 1);

        var searchBox = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = 14 },
            Padding = new Thickness(16, 8),
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.12f,
                Radius = 10,
                Offset = new Point(0, 3)
            },
            Content = searchGrid,
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center
        };

        _refreshTopButton = new Button
        {
            Text = "⟳",
            FontSize = 20,
            BackgroundColor = Colors.White,
            TextColor = Color.FromArgb("#0F766E"),
            WidthRequest = 52,
            HeightRequest = 52,
            CornerRadius = 14,
            Padding = 0,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.12f,
                Radius = 10,
                Offset = new Point(0, 3)
            }
        };
        _refreshTopButton.Clicked += OnRefreshClicked;

        _myLocationButton = new Button
        {
            Text = "◎",
            FontSize = 20,
            BackgroundColor = Colors.White,
            TextColor = Color.FromArgb("#2563EB"),
            WidthRequest = 52,
            HeightRequest = 52,
            CornerRadius = 14,
            Padding = 0,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.12f,
                Radius = 10,
                Offset = new Point(0, 3)
            }
        };
        _myLocationButton.Clicked += OnMyLocationClicked;

        _teleportButton = new Button
        {
            Text = "TP",
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Colors.White,
            TextColor = Color.FromArgb("#7C3AED"),
            WidthRequest = 52,
            HeightRequest = 44,
            CornerRadius = 12,
            Padding = 0,
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.12f,
                Radius = 10,
                Offset = new Point(0, 3)
            }
        };
        _teleportButton.Clicked += OnTeleportClicked;

        var topBar = new Grid
        {
            Padding = new Thickness(16, 10, 16, 0),
            ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto }
            },
            ColumnSpacing = 10,
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.Fill,
            ZIndex = 20
        };

        topBar.Children.Add(searchBox);
        Grid.SetColumn(searchBox, 0);

        var rightButtons = new VerticalStackLayout
        {
            Spacing = 8,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions = LayoutOptions.Start,
            Children = { _refreshTopButton, _myLocationButton, _teleportButton }
        };

        topBar.Children.Add(rightButtons);
        Grid.SetColumn(rightButtons, 1);

        return topBar;
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

    private void OnMapPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Map.VisibleRegion))
            ScheduleVisiblePinRefresh();
    }

    private void ScheduleVisiblePinRefresh()
    {
        _pinRefreshCts?.Cancel();
        _pinRefreshCts?.Dispose();
        _pinRefreshCts = new CancellationTokenSource();
        var ct = _pinRefreshCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(120, ct);
                if (ct.IsCancellationRequested)
                    return;

                await MainThread.InvokeOnMainThreadAsync(RefreshVisiblePins);
            }
            catch (OperationCanceledException)
            {
            }
        }, ct);
    }

    private void RefreshVisiblePins()
    {
        if (_pinsByPoiId.Count == 0)
            return;

        var visiblePoiIds = GetVisiblePoiIds();
        var desiredPins = visiblePoiIds
            .Select(id => _pinsByPoiId.TryGetValue(id, out var pin) ? pin : null)
            .Where(pin => pin is not null)
            .Cast<StyledPin>()
            .ToHashSet();

        var currentStyledPins = _map.Pins.OfType<StyledPin>().ToList();
        foreach (var pin in currentStyledPins)
        {
            if (!desiredPins.Contains(pin))
                _map.Pins.Remove(pin);
        }

        foreach (var pin in desiredPins)
        {
            if (!_map.Pins.Contains(pin))
                _map.Pins.Add(pin);
        }
    }

    private HashSet<int> GetVisiblePoiIds()
    {
        var result = new HashSet<int>();
        var region = _map.VisibleRegion;

        if (region is null)
        {
            foreach (var poi in _pois.Take(40))
                result.Add(poi.IDChiNhanh);

            return result;
        }

        var centerLat = region.Center.Latitude;
        var centerLon = region.Center.Longitude;
        var latRadius = region.LatitudeDegrees / 2d;
        var lonRadius = region.LongitudeDegrees / 2d;
        const double overscanFactor = 1.35;

        foreach (var poi in _pois)
        {
            if (Math.Abs(poi.Latitude - centerLat) <= latRadius * overscanFactor &&
                Math.Abs(poi.Longitude - centerLon) <= lonRadius * overscanFactor)
            {
                result.Add(poi.IDChiNhanh);
            }
        }

        if (result.Count == 0)
        {
            foreach (var poi in _pois.Take(25))
                result.Add(poi.IDChiNhanh);
        }

        return result;
    }

    private bool HasInternet()
    {
        return Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
    }

    private async Task InitializePageAsync()
    {
        try
        {
            await EnsureExploreSheetVisibleAsync();
            await LoadLanguagesAsync();
            await LoadRealPoisAsync();
            _ = DiagnosePoiImagesAsync();
            await InitializeGeofenceAsync();
            _isInitialLoadCompleted = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PoiMapPage] Initial load error: {ex.Message}");
        }
    }

    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        try
        {
            _refreshTopButton.IsEnabled = false;
            _refreshTopButton.Text = "...";

            if (!HasInternet())
            {
                await DisplayAlertAsync("Thông báo", "Không có mạng để tải lại dữ liệu.", "OK");
                return;
            }

            await LoadRealPoisAsync();

            if (_currentDetailGianHang != null)
            {
                var refreshed = await _gianHangService.GetByIdAsync(_currentDetailGianHang.IdGianHang, _selectedLanguageCode);
                if (refreshed != null)
                {
                    _currentDetailGianHang = refreshed;
                    SetDetailInfo(refreshed);

                    _detailAudioLabel.Text = string.IsNullOrWhiteSpace(refreshed.AudioURL)
                        ? "Audio: chưa có"
                        : "Audio thuyết minh đã sẵn sàng";

                    if (!string.IsNullOrWhiteSpace(refreshed.HinhAnhChinh) || !string.IsNullOrWhiteSpace(refreshed.HinhAnh))
                    {
                        _detailImage.Source = BuildImageSource(
                            !string.IsNullOrWhiteSpace(refreshed.HinhAnhChinh)
                                ? refreshed.HinhAnhChinh
                                : refreshed.HinhAnh);
                    }
                }
            }

        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi refresh", ex.Message, "OK");
        }
        finally
        {
            _refreshTopButton.IsEnabled = true;
            _refreshTopButton.Text = "⟳";
        }
    }

    private async void OnMyLocationClicked(object? sender, EventArgs e)
    {
        try
        {
            _myLocationButton.IsEnabled = false;
            _myLocationButton.Text = "...";
            await ShowCurrentLocationMarkerAsync(centerOnUser: true);
        }
        finally
        {
            _myLocationButton.IsEnabled = true;
            _myLocationButton.Text = "◎";
        }
    }

    private async void OnTeleportClicked(object? sender, EventArgs e)
    {
        try
        {
            _teleportButton.IsEnabled = false;

            var latText = await DisplayPromptAsync(
                "Teleport",
                "Nhập latitude",
                initialValue: "10.762622",
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(latText))
                return;

            var lonText = await DisplayPromptAsync(
                "Teleport",
                "Nhập longitude",
                initialValue: "106.660172",
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(lonText))
                return;

            if (!double.TryParse(latText.Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat) ||
                !double.TryParse(lonText.Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lon))
            {
                await DisplayAlertAsync("Teleport", "Tọa độ không hợp lệ.", "OK");
                return;
            }

            var fakeLocation = new Location(lat, lon);
            UpdateUserLocationPin(fakeLocation, centerOnUser: true);
            _geofenceEngine.SetDebugLocation(lat, lon);
            await InitializeGeofenceAsync();
            await _geofenceEngine.EvaluateNowAsync();

            System.Diagnostics.Debug.WriteLine($"[Teleport] Debug location set to lat={lat}, lon={lon}");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Teleport", ex.Message, "OK");
        }
        finally
        {
            _teleportButton.IsEnabled = true;
        }
    }

    private async Task LoadRealPoisAsync()
    {
        if (_isLoadingPois)
            return;

        _isLoadingPois = true;

        try
        {
            _pois.Clear();
            _poiList.Children.Clear();
            _map.Pins.Clear();
            _pinsByPoiId.Clear();

            var data = await _poiService.GetAllPoisAsync();

            System.Diagnostics.Debug.WriteLine($"[PoiMapPage] Loaded {data.Count} POIs from database");

            foreach (var poi in data)
            {
                System.Diagnostics.Debug.WriteLine($"[PoiMapPage] POI: {poi.Title}, ImagePath: '{poi.ImagePath}'");

                _pois.Add(poi);
                _poiList.Children.Add(CreatePoiCard(poi));
                var markerImagePath = await PrepareMarkerImagePathAsync(poi.ImagePath);

                var pin = new StyledPin
                {
                    Label = poi.Title,
                    Address = poi.Subtitle,
                    Type = PinType.Place,
                    Location = new Location(poi.Latitude, poi.Longitude),
                    Rating = 4.9,
                    ImagePath = markerImagePath
                };

                pin.MarkerClicked += async (_, e) =>
                {
                    e.HideInfoWindow = true;
                    await OpenDetailAsync(poi);
                };

                _pinsByPoiId[poi.IDChiNhanh] = pin;

                if (_pois.Count % 6 == 0)
                    await Task.Yield();
            }

            if (_pois.Count > 0)
            {
                var first = _pois[0];
                _map.MoveToRegion(
                    MapSpan.FromCenterAndRadius(
                        new Location(first.Latitude, first.Longitude),
                        Distance.FromKilometers(1)));
            }

            RefreshVisiblePins();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Lỗi", $"Không tải được dữ liệu POI từ DB.\n{ex.Message}", "OK");
        }
        finally
        {
            _isLoadingPois = false;

            if (_isInitialLoadCompleted)
                await ShowCurrentLocationMarkerAsync(centerOnUser: false);
        }
    }

    private Grid CreateBottomSheet()
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
            HorizontalOptions = LayoutOptions.Start
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
            IsVisible = false
        };
    }

    private Grid CreateDetailSheet()
    {
        _detailImage = new Image
        {
            Source = "dotnet_bot.png",
            Aspect = Aspect.AspectFill,
            HeightRequest = 250
        };

        var imageHeader = new Border
        {
            StrokeThickness = 0,
            HeightRequest = 250,
            StrokeShape = new RoundRectangle
            {
                CornerRadius = new CornerRadius(28, 28, 0, 0)
            },
            Content = _detailImage
        };

        _detailTitle = new Label
        {
            Text = "Tên gian hàng",
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black
        };

        _detailAddress = new Label
        {
            Text = "Địa chỉ",
            FontSize = 14,
            TextColor = Colors.Gray
        };

        _detailDescription = new Label
        {
            Text = "Mô tả gian hàng",
            FontSize = 15,
            TextColor = Colors.Black,
            LineBreakMode = LineBreakMode.WordWrap
        };

        _detailAudioLabel = new Label
        {
            Text = "Audio: chưa có",
            FontSize = 13,
            TextColor = Colors.Gray
        };

        var closeButton = new Button
        {
            Text = "Đóng",
            BackgroundColor = Color.FromArgb("#E85D04"),
            TextColor = Colors.White,
            CornerRadius = 12,
            Padding = new Thickness(16, 10)
        };
        closeButton.Clicked += async (_, __) => await HideDetailSheetAsync();

        var dragBar = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#D4D4D8"),
            StrokeShape = new RoundRectangle { CornerRadius = 999 },
            HeightRequest = 5,
            WidthRequest = 48,
            HorizontalOptions = LayoutOptions.Center,
            Margin = new Thickness(0, 10, 0, 10)
        };

        var dragArea = new Grid
        {
            HeightRequest = 34,
            Children = { dragBar }
        };

        var pan = new PanGestureRecognizer();
        pan.PanUpdated += OnDetailPanUpdated;
        dragArea.GestureRecognizers.Add(pan);

        var infoCard = new Border
        {
            Stroke = Color.FromArgb("#EEEEEE"),
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            BackgroundColor = Colors.White,
            Padding = 16,
            Margin = new Thickness(16, -20, 16, 0),
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    _detailTitle,
                    _detailAddress,
                    _detailDescription,
                    _detailAudioLabel
                }
            }
        };

        _languageRow = new HorizontalStackLayout
        {
            Spacing = 8
        };

        var languageCard = new Border
        {
            Stroke = Color.FromArgb("#EEEEEE"),
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            BackgroundColor = Colors.White,
            Padding = 16,
            Margin = new Thickness(16, 0, 16, 0),
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Label
                    {
                        Text = "Ngôn ngữ",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.Black
                    },
                    new ScrollView
                    {
                        Orientation = ScrollOrientation.Horizontal,
                        HorizontalScrollBarVisibility = ScrollBarVisibility.Never,
                        Content = _languageRow
                    }
                }
            }
        };

        _playButton = new Button
        {
            Text = "▶ Phát nè",
            BackgroundColor = Color.FromArgb("#E85D04"),
            TextColor = Colors.White,
            CornerRadius = 12,
            Padding = new Thickness(16, 10)
        };
        _playButton.Clicked += OnPlayButtonClicked;

        _progressSlider = new Slider
        {
            Minimum = 0,
            Maximum = 1,
            Value = 0
        };
        _progressSlider.DragStarted += (_, __) => _isDraggingSlider = true;
        _progressSlider.DragCompleted += OnProgressDragCompleted;

        _currentTimeLabel = new Label
        {
            Text = "00:00",
            FontSize = 12,
            TextColor = Colors.Gray
        };

        _durationLabel = new Label
        {
            Text = "00:00",
            FontSize = 12,
            TextColor = Colors.Gray,
            HorizontalOptions = LayoutOptions.End
        };

        var timeGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        timeGrid.Children.Add(_currentTimeLabel);
        Grid.SetColumn(_currentTimeLabel, 0);

        timeGrid.Children.Add(_durationLabel);
        Grid.SetColumn(_durationLabel, 1);

        var audioCard = new Border
        {
            Stroke = Color.FromArgb("#EEEEEE"),
            StrokeShape = new RoundRectangle { CornerRadius = 20 },
            BackgroundColor = Colors.White,
            Padding = 16,
            Margin = new Thickness(16, 0, 16, 0),
            Content = new VerticalStackLayout
            {
                Spacing = 12,
                Children =
                {
                    new Label
                    {
                        Text = "Thuyết minh audio",
                        FontSize = 18,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Colors.Black
                    },
                    _playButton,
                    _progressSlider,
                    timeGrid
                }
            }
        };

        var foodTitle = new Label
        {
            Text = "Một số hình ảnh món ăn",
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            TextColor = Colors.Black,
            Margin = new Thickness(16, 0, 16, 0)
        };

        var foodImages = new HorizontalStackLayout
        {
            Spacing = 12,
            Padding = new Thickness(16, 0, 16, 0),
            Children =
            {
                CreateDemoFoodImage(),
                CreateDemoFoodImage(),
                CreateDemoFoodImage()
            }
        };

        var content = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 14,
                Padding = new Thickness(0, 0, 0, 300),
                Children =
                {
                    imageHeader,
                    infoCard,
                    languageCard,
                    audioCard,
                    foodTitle,
                    foodImages,
                    new VerticalStackLayout
                    {
                        Padding = new Thickness(16, 0, 16, 0),
                        Children = { closeButton }
                    }
                }
            }
        };

        var body = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition { Height = GridLength.Auto },
                new RowDefinition { Height = GridLength.Star }
            }
        };

        body.Children.Add(dragArea);
        body.Children.Add(content);
        Grid.SetRow(content, 1);

        _detailPanel = new Border
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
                Opacity = 0.14f,
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
            Children = { _detailPanel },
            IsVisible = false,
            InputTransparent = false
        };
    }

    private View CreateDemoFoodImage()
    {
        return new Border
        {
            Stroke = Color.FromArgb("#EEEEEE"),
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            HeightRequest = 100,
            WidthRequest = 140,
            Content = new Image
            {
                Source = "dotnet_bot.png",
                Aspect = Aspect.AspectFill
            }
        };
    }

    private View BuildFooter()
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

        var home = BuildFooterItem("🏠", "Trang chủ", false, async () =>
        {
            var homePage = App.Current?.Handler?.MauiContext?.Services.GetRequiredService<HomePage>();
            if (homePage != null)
            {
                await Navigation.PushAsync(homePage);
            }
        });

        var explore = BuildFooterItem("🧭", "Khám phá", true, ToggleSuggestionSheetAsync);

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            Padding = new Thickness(20, 12)
        };

        grid.Children.Add(home);

        grid.Children.Add(explore);
        Grid.SetColumn(explore, 1);

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

        if (_isDetailVisible)
        {
            await HideDetailSheetAsync();
        }

        if (!_isExploreVisible)
        {
            ResetExploreHeader();
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
            await _bottomSheet.TranslateToAsync(0, _sheetHiddenY, 180, Easing.CubicIn);

            _currentSheetY = _sheetHiddenY;
            _bottomSheet.TranslationY = _sheetHiddenY;
            _bottomSheet.IsVisible = false;
            _isExploreVisible = false;
            ResetExploreHeader();

            UpdateScrollAvailability();
            UpdateHeaderByState();
#if ANDROID
            UpdateAndroidMapPadding();
#endif
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
        SetDetailInfo(gianHang);

        ResetAudioState();

        _detailAudioLabel.Text = string.IsNullOrWhiteSpace(gianHang.AudioURL)
            ? "Audio: chưa có"
            : "Audio thuyết minh đã sẵn sàng";

        _detailImage.Source = BuildImageSource(
            !string.IsNullOrWhiteSpace(gianHang.HinhAnhChinh)
                ? gianHang.HinhAnhChinh
                : gianHang.HinhAnh);

        await LoadLanguagesAsync(forceReload: true);
        RenderLanguageOptions();
        await ApplySelectedLanguageToCurrentDetailAsync();

        _detailSheet.IsVisible = true;
        _detailSheet.TranslationY = _detailHiddenY;
        _detailCurrentY = _detailHiddenY;
        _isDetailVisible = true;

        await SnapDetailSheetToAsync(_detailHalfY);
#if ANDROID
        UpdateAndroidMapPadding();
#endif
    }

    private async Task HideDetailSheetAsync()
    {
        if (!_isDetailVisible || _isDetailAnimating)
            return;

        _isDetailAnimating = true;

        try
        {
            StopAndDisposeAudio();

            await _detailSheet.TranslateToAsync(0, _detailHiddenY, 180, Easing.CubicIn);

            _detailCurrentY = _detailHiddenY;
            _detailSheet.TranslationY = _detailHiddenY;
            _detailSheet.IsVisible = false;
            _isDetailVisible = false;
#if ANDROID
            UpdateAndroidMapPadding();
#endif
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

        _detailFullY = 0;
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
#if ANDROID
        UpdateAndroidMapPadding();
#endif
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
                nextY = Math.Max(_sheetFullY, Math.Min(_sheetHiddenY, nextY));

                _currentSheetY = nextY;
                _bottomSheet.TranslationY = _currentSheetY;

                UpdateScrollAvailability();
#if ANDROID
                UpdateAndroidMapPadding();
#endif
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
                UpdateDetailPanelShape(_detailLastPanDeltaY < -0.05);
#if ANDROID
                UpdateAndroidMapPadding();
#endif
                break;

            case GestureStatus.Canceled:
            case GestureStatus.Completed:
                var target = ResolveDetailSnapTargetWithDirection(_detailCurrentY, _detailLastPanDeltaY);
                if (target >= _detailHiddenY - 1)
                {
                    _ = HideDetailSheetAsync();
                }
                else if (target <= _detailFullY + 2)
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
            await SnapDetailSheetToAsync(_detailFullY);
            UpdateDetailPanelShape(true);

            var page = new GianHangFoodGalleryPage(
                _currentDetailGianHang,
                _monAnService,
                NormalizeImagePath(_currentDetailGianHang.HinhAnh));

            var isMenuLayerOpened = false;
            var menuOpenedTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            void OnMenuLayerLoaded(object? sender, EventArgs args)
            {
                isMenuLayerOpened = true;
                menuOpenedTcs.TrySetResult(true);
            }

            page.Loaded += OnMenuLayerLoaded;

            try
            {
                await Navigation.PushAsync(page);

                if (!isMenuLayerOpened)
                    await menuOpenedTcs.Task;
            }
            finally
            {
                page.Loaded -= OnMenuLayerLoaded;
            }

            if (!isMenuLayerOpened)
                return;

            StopAndDisposeAudio();

            _detailCurrentY = _detailHiddenY;
            _detailSheet.TranslationY = _detailHiddenY;
            _detailSheet.Opacity = 1;
            _detailSheet.IsVisible = false;
            _detailSheet.InputTransparent = false;
            _isDetailVisible = false;
            UpdateDetailPanelShape();
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

            await _bottomSheet.TranslateToAsync(0, targetY, 180, Easing.CubicOut);

            _currentSheetY = targetY;
            _bottomSheet.TranslationY = targetY;

            UpdateScrollAvailability();
            UpdateHeaderByState();
#if ANDROID
            UpdateAndroidMapPadding();
#endif
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

            await _detailSheet.TranslateToAsync(0, targetY, 180, Easing.CubicOut);

            _detailCurrentY = targetY;
            _detailSheet.TranslationY = targetY;
            UpdateDetailPanelShape();
#if ANDROID
            UpdateAndroidMapPadding();
#endif
        }
        finally
        {
            _isDetailAnimating = false;
        }
    }

#if ANDROID
    private void TryInitializeAndroidMap()
    {
        if (_androidGoogleMap is not null)
            return;

        if (_map.Handler is not IMapHandler mapHandler || mapHandler.PlatformView is null)
            return;

        mapHandler.PlatformView.GetMapAsync(new MapReadyCallback(map =>
        {
            _androidGoogleMap = map;
            _androidGoogleMap.UiSettings.ZoomControlsEnabled = true;
            UpdateAndroidMapPadding();
        }));
    }

    private void UpdateAndroidMapPadding()
    {
        if (_androidGoogleMap is null || Height <= 0)
            return;

        var overlayTop = Height;

        if (_footer.IsVisible)
            overlayTop = Math.Min(overlayTop, Height - _footer.Height);

        if (_bottomSheet.IsVisible)
        {
            // Giữ vị trí zoom controls ổn định như trước: không trôi xuống đáy khi sheet hạ thấp.
            var fixedExploreTop = _sheetHalfY;
            overlayTop = Math.Min(overlayTop, fixedExploreTop);
        }

        if (_detailSheet.IsVisible)
            overlayTop = Math.Min(overlayTop, _detailCurrentY);

        var bottomInsetDip = Math.Max(0, Height - overlayTop + 8);
        var bottomInsetPx = (int)Math.Ceiling(bottomInsetDip * DeviceDisplay.MainDisplayInfo.Density);

        _androidGoogleMap.SetPadding(0, 0, 0, bottomInsetPx);
    }

    private sealed class MapReadyCallback(Action<GoogleMap> onMapReady) : Java.Lang.Object, IOnMapReadyCallback
    {
        public void OnMapReady(GoogleMap googleMap) => onMapReady(googleMap);
    }
#endif

    public void RequestAutoOpenExplore()
    {
        _shouldAutoOpenExplore = true;
    }

    private async Task EnsureExploreSheetVisibleAsync()
    {
        if (!_shouldAutoOpenExplore || !_isLayoutReady || _isAnimating)
            return;

        if (_isDetailVisible)
        {
            await HideDetailSheetAsync();
        }

        if (!_isExploreVisible)
        {
            ResetExploreHeader();
            _bottomSheet.IsVisible = true;
            _bottomSheet.TranslationY = _sheetHiddenY;
            _currentSheetY = _sheetHiddenY;
            _isExploreVisible = true;
        }

        await SnapSheetToAsync(_sheetHalfY);
        _shouldAutoOpenExplore = false;
    }

    private void SetDetailInfo(GianHang gianHang)
    {
        _detailTitle.Text = string.IsNullOrWhiteSpace(gianHang.Ten) ? "Tên gian hàng" : gianHang.Ten;
        _detailAddress.Text = string.IsNullOrWhiteSpace(gianHang.DiaChi) ? "Chưa có địa chỉ" : gianHang.DiaChi;
        _detailDescription.Text = string.IsNullOrWhiteSpace(gianHang.MoTa) ? "Chưa có mô tả." : gianHang.MoTa;
    }

    private void UpdateDetailPanelShape(bool forceSquare = false)
    {
        var topRadius = forceSquare || _detailCurrentY <= _detailFullY + 2 ? 0 : 28;
        _detailPanel.StrokeShape = new RoundRectangle
        {
            CornerRadius = new CornerRadius(topRadius, topRadius, 0, 0)
        };
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

    private void ResetExploreHeader()
    {
        _titleLabel.Text = "Khám phá";
        _subtitleLabel.Text = "Ẩm thực, đồ uống và các địa điểm gần bạn";
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
            Source = BuildImageSource(poi.ImagePath),
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
            var gianHang = await _gianHangService.GetByIdAsync(poi.IDChiNhanh);

            if (gianHang == null)
            {
                await DisplayAlertAsync("Thông báo", $"Không tìm thấy gian hàng tương ứng. Id = {poi.IDChiNhanh}", "OK");
                return;
            }

            await FocusPoiAsync(poi);
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

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _pinRefreshCts?.Cancel();
        _pinRefreshCts?.Dispose();
        _pinRefreshCts = null;
        StopAndDisposeAudio();
        _ = _geofenceEngine.StopAsync();
    }
}

