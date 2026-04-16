using System.Linq;
using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Maui.Controls.Shapes;

namespace MauiApp1.Views;

public class GianHangFoodGalleryPage : ContentPage
{
    private readonly GianHang _gianHang;
    private readonly MonAnService _monAnService;
    private readonly string _heroImage;

    private readonly Label _heroSubtitleLabel;
    private readonly Label _summaryLabel;
    private readonly Label _menuActionLabel;
    private readonly Grid _featuredHeader;
    private readonly Grid _menuHeader;
    private readonly Border _featuredCard;
    private readonly Border _statusCard;
    private readonly Label _statusLabel;
    private readonly VerticalStackLayout _menuList;
    private readonly VerticalStackLayout _popularList;
    private readonly HorizontalStackLayout _drinkRow;

    private bool _isLoaded;

    public GianHangFoodGalleryPage(GianHang gianHang, MonAnService monAnService, string? heroImage)
    {
        _gianHang = gianHang;
        _monAnService = monAnService;
        _heroImage = string.IsNullOrWhiteSpace(heroImage)
            ? (_gianHang.HinhAnhFullUrl ?? "dotnet_bot.png")
            : heroImage;

        NavigationPage.SetHasNavigationBar(this, false);
        BackgroundColor = Colors.White;

        _heroSubtitleLabel = new Label
        {
            Text = BuildHeroSubtitle(0),
            FontSize = 12,
            TextColor = Colors.White
        };

        _summaryLabel = new Label
        {
            Text = BuildSummaryText(0),
            FontSize = 14,
            TextColor = Color.FromArgb("#5C5C5C")
        };

        _menuActionLabel = new Label
        {
            Text = "0 mon",
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FF6B00"),
            VerticalTextAlignment = TextAlignment.End
        };

        _featuredHeader = BuildSectionTitle("Mon noi bat", (Label?)null);
        _menuHeader = BuildSectionTitle("Thuc don", _menuActionLabel);

        _featuredCard = new Border
        {
            Stroke = Color.FromArgb("#EDEDED"),
            StrokeShape = new RoundRectangle { CornerRadius = 26 },
            Padding = 0,
            BackgroundColor = Colors.White,
            IsVisible = false,
            Content = CreateDishCard("Món nổi bật", "Mô tả đang cập nhật", 0, true)
        };

        _statusLabel = new Label
        {
            FontSize = 14,
            TextColor = Color.FromArgb("#5C5C5C")
        };

        _statusCard = new Border
        {
            Stroke = Color.FromArgb("#EDEDED"),
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            BackgroundColor = Color.FromArgb("#FAFAFA"),
            Padding = 16,
            Content = _statusLabel
        };

        _menuList = new VerticalStackLayout { Spacing = 16 };
        _popularList = new VerticalStackLayout { Spacing = 16 };
        _drinkRow = new HorizontalStackLayout { Spacing = 12 };

        var layout = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Auto),
                new RowDefinition(GridLength.Star)
            }
        };

        var header = BuildHeader();
        var scroll = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 20,
                Padding = new Thickness(16, 6, 16, 24),
                Children =
                {
                    BuildHero(),
                    CreateSummaryCard(),
                    _featuredHeader,
                    _featuredCard,
                    _statusCard,
                    BuildSectionTitle("Món chính phổ biến", "Xem tất cả"),
                    _popularList,
                    new Label
                    {
                        Text = "Đồ uống đặc sắc",
                        FontSize = 32,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#1F1F1F")
                    },
                    new ScrollView
                    {
                        Orientation = ScrollOrientation.Horizontal,
                        Content = _drinkRow
                    }
                }
            }
        };

        layout.Children.Add(header);
        layout.Children.Add(scroll);
        Grid.SetRow(scroll, 1);

        Content = layout;
        ShowStatus("Dang tai thuc don tu backend...");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoaded)
            return;

        _isLoaded = true;

        try
        {
            await LoadMenuAsync();
        }
        catch (Exception ex)
        {
            _isLoaded = false;
            ShowStatus("Khong tai duoc thuc don cua gian hang nay.");
            System.Diagnostics.Debug.WriteLine($"[GianHangFoodGalleryPage] Load menu error: {ex.Message}");
        }
    }

    private View BuildHeader()
    {
        var backIcon = new Label
        {
            Text = "←",
            FontSize = 24,
            TextColor = Color.FromArgb("#FF6B00"),
            VerticalTextAlignment = TextAlignment.Center
        };

        var tap = new TapGestureRecognizer();
        tap.Tapped += async (_, __) => await Navigation.PopAsync();
        backIcon.GestureRecognizers.Add(tap);

        var title = new Label
        {
            Text = string.IsNullOrWhiteSpace(_gianHang.Ten) ? "Thuc don" : _gianHang.Ten,
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#1F1F1F"),
            LineBreakMode = LineBreakMode.TailTruncation,
            MaxLines = 1
        };

        var avatar = new Border
        {
            HeightRequest = 38,
            WidthRequest = 38,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 19 },
            BackgroundColor = Color.FromArgb("#FFF1E8"),
            Content = new Label
            {
                Text = GetAvatarText(),
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#FF6B00"),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        var grid = new Grid
        {
            Padding = new Thickness(16, 16, 16, 6),
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };

        grid.Children.Add(backIcon);
        grid.Children.Add(title);
        Grid.SetColumn(title, 1);
        grid.Children.Add(avatar);
        Grid.SetColumn(avatar, 2);

        return grid;
    }

    private View BuildHero()
    {
        return new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 26 },
            HeightRequest = 280,
            Content = new Grid
            {
                Children =
                {
                    new Image
                    {
                        Source = BuildImageSource(_heroImage),
                        Aspect = Aspect.AspectFill
                    },
                    new Border
                    {
                        StrokeThickness = 0,
                        BackgroundColor = Color.FromRgba(0, 0, 0, 0.38),
                        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0, 0, 26, 26) },
                        VerticalOptions = LayoutOptions.End,
                        Padding = 16,
                        Content = new VerticalStackLayout
                        {
                            Spacing = 6,
                            Children =
                            {
                                new Border
                                {
                                    StrokeThickness = 0,
                                    BackgroundColor = Color.FromArgb("#FF6B00"),
                                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                                    Padding = new Thickness(10, 4),
                                    HorizontalOptions = LayoutOptions.Start,
                                    Content = new Label
                                    {
                                        Text = "THUC DON GIAN HANG",
                                        FontSize = 11,
                                        FontAttributes = FontAttributes.Bold,
                                        TextColor = Colors.White
                                    }
                                },
                                new Label
                                {
                                    Text = string.IsNullOrWhiteSpace(_gianHang.Ten) ? "Nhà hàng" : _gianHang.Ten,
                                    FontSize = 36,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = Colors.White
                                },
                                new Label
                                {
                                    Text = "⭐ 4.9 (2.4k reviews)  ·  ⏰ 07:00 - 22:00",
                                    FontSize = 12,
                                    TextColor = Colors.White,
                                    IsVisible = false
                                },
                                _heroSubtitleLabel
                            }
                        }
                    }
                }
            }
        };
    }

    private View BuildCategoryChips()
    {
        var row = new HorizontalStackLayout { Spacing = 10 };
        row.Children.Add(CreateChip("Món chính", true));
        row.Children.Add(CreateChip("Khai vị", false));
        row.Children.Add(CreateChip("Đồ uống", false));
        return row;
    }

    private View CreateChip(string text, bool active)
    {
        return new Border
        {
            StrokeThickness = 0,
            BackgroundColor = active ? Color.FromArgb("#FF6B00") : Color.FromArgb("#EFEFEF"),
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(14, 8),
            Content = new Label
            {
                Text = text,
                FontSize = 13,
                FontAttributes = FontAttributes.Bold,
                TextColor = active ? Colors.White : Color.FromArgb("#595959")
            }
        };
    }

    private Grid BuildSectionTitle(string title, Label? actionLabel)
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        grid.Children.Add(new Label
        {
            Text = title,
            FontSize = 28,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#1F1F1F")
        });

        if (actionLabel is not null)
        {
            grid.Children.Add(actionLabel);
            Grid.SetColumn(actionLabel, 1);
        }

        return grid;
    }

    private Grid BuildSectionTitle(string title, string action)
    {
        return BuildSectionTitle(title, new Label
        {
            Text = action,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FF6B00"),
            VerticalTextAlignment = TextAlignment.End
        });
    }

    private Border CreateSummaryCard()
    {
        return new Border
        {
            Stroke = Color.FromArgb("#ECECEC"),
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            BackgroundColor = Color.FromArgb("#FFF8F1"),
            Padding = 16,
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Label
                    {
                        Text = "Du lieu thuc don",
                        FontSize = 12,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#FF6B00")
                    },
                    _summaryLabel
                }
            }
        };
    }

    private async Task LoadMenuAsync()
    {
        var items = FilterMenuItems(await _monAnService.GetByGianHangAsync(_gianHang.IdGianHang));

        if (items.Count == 0 && _gianHang.MonAns.Count > 0)
            items = FilterMenuItems(_gianHang.MonAns);

        _summaryLabel.Text = BuildSummaryText(items.Count);
        _heroSubtitleLabel.Text = BuildHeroSubtitle(items.Count);
        _menuActionLabel.Text = $"{items.Count} mon";

        _popularList.Children.Clear();
        _drinkRow.Children.Clear();

        if (items.Count == 0)
        {
            _featuredHeader.IsVisible = false;
            _featuredCard.IsVisible = false;
            ShowStatus("Gian hang nay chua co mon nao dang ban tren backend.");
            return;
        }

        _featuredHeader.IsVisible = true;
        _featuredCard.IsVisible = true;
        HideStatus();
        _featuredCard.Content = CreateDishCard(items[0], true);

        foreach (var item in items.Skip(1).Take(3))
        {
            _popularList.Children.Add(CreateDishCard(item, false));
        }

        foreach (var item in items.Skip(4).Take(4))
        {
            _drinkRow.Children.Add(CreateDrinkCard(item));
        }

        if (_popularList.Children.Count == 0)
            ShowStatus("Gian hang hien co 1 mon dang ban.");
    }

    private View CreateDishCard(string? name, string? description, decimal price, bool large)
    {
        return CreateDishCard(new MonAn
        {
            TenMon = name ?? string.Empty,
            ThongTinMon = description ?? string.Empty,
            DonGia = price
        }, large);
    }

    private View CreateDishCard(MonAn item, bool large)
    {
        var price = item.DonGia;
        var name = item.TenMon;
        var description = item.ThongTinMon;
        var imageSource = item.HinhAnhFullUrl;

        var priceGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        var priceLabel = new Label
        {
            Text = $"{price:N0}đ",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FF6B00")
        };

        var addButton = new Border
        {
            StrokeThickness = 0,
            HeightRequest = 32,
            WidthRequest = 32,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            BackgroundColor = Color.FromArgb("#FF6B00"),
            Content = new Label
            {
                Text = "+",
                FontSize = 18,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            }
        };

        priceGrid.Children.Add(priceLabel);
        priceGrid.Children.Add(addButton);
        Grid.SetColumn(addButton, 1);

        return new Border
        {
            Stroke = Color.FromArgb("#ECECEC"),
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            BackgroundColor = Colors.White,
            Padding = 12,
            Content = new VerticalStackLayout
            {
                Spacing = 10,
                Children =
                {
                    new Border
                    {
                        StrokeThickness = 0,
                        StrokeShape = new RoundRectangle { CornerRadius = 20 },
                        HeightRequest = large ? 210 : 130,
                        Content = new Image
                        {
                            Source = BuildImageSource(imageSource),
                            Aspect = Aspect.AspectFill
                        }
                    },
                    new Label
                    {
                        Text = string.IsNullOrWhiteSpace(name) ? "Món ăn" : name,
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#1F1F1F")
                    },
                    new Label
                    {
                        Text = string.IsNullOrWhiteSpace(description) ? "Món ngon đặc trưng của gian hàng." : description,
                        FontSize = 13,
                        TextColor = Color.FromArgb("#707070"),
                        LineBreakMode = LineBreakMode.TailTruncation,
                        MaxLines = 2
                    },
                    priceGrid
                }
            }
        };
    }

    private View CreateDrinkCard(MonAn item)
    {
        var name = item.TenMon;
        var price = item.DonGia;
        var imageSource = item.HinhAnhFullUrl;

        return new Border
        {
            Stroke = Color.FromArgb("#ECECEC"),
            StrokeShape = new RoundRectangle { CornerRadius = 22 },
            BackgroundColor = Color.FromArgb("#F7F7F7"),
            WidthRequest = 150,
            Padding = new Thickness(12),
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Border
                    {
                        Stroke = Color.FromArgb("#FF6B00"),
                        StrokeShape = new RoundRectangle { CornerRadius = 30 },
                        HeightRequest = 60,
                        WidthRequest = 60,
                        HorizontalOptions = LayoutOptions.Center,
                        Content = new Image
                        {
                            Source = BuildImageSource(imageSource),
                            Aspect = Aspect.AspectFill
                        }
                    },
                    new Label
                    {
                        Text = string.IsNullOrWhiteSpace(name) ? "Đồ uống" : name,
                        FontSize = 13,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextColor = Color.FromArgb("#333")
                    },
                    new Label
                    {
                        Text = $"{price:N0}đ",
                        FontSize = 14,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalTextAlignment = TextAlignment.Center,
                        TextColor = Color.FromArgb("#FF6B00")
                    }
                }
            }
        };
    }

    private void ShowStatus(string message)
    {
        _statusLabel.Text = message;
        _statusCard.IsVisible = true;
    }

    private void HideStatus()
    {
        _statusCard.IsVisible = false;
    }

    private static List<MonAn> FilterMenuItems(IEnumerable<MonAn>? items)
    {
        if (items is null)
            return new List<MonAn>();

        return items
            .Where(item => item is not null)
            .Where(item =>
                string.IsNullOrWhiteSpace(item.TinhTrang) ||
                string.Equals(item.TinhTrang, "con_ban", StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.IdMonAn)
            .ToList();
    }

    private string BuildSummaryText(int itemCount)
    {
        if (itemCount <= 0)
            return "Chua nhan duoc mon dang ban cho gian hang nay.";

        var storeName = string.IsNullOrWhiteSpace(_gianHang.Ten) ? "Gian hang" : _gianHang.Ten;
        return $"{storeName} hien co {itemCount} mon dang ban duoc dong bo tu backend.";
    }

    private string BuildHeroSubtitle(int itemCount)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(_gianHang.DiaChi))
            parts.Add(_gianHang.DiaChi!);

        parts.Add(itemCount <= 0 ? "Chua co mon dang ban" : $"{itemCount} mon dang ban");
        return string.Join(" | ", parts);
    }

    private string GetAvatarText()
    {
        if (string.IsNullOrWhiteSpace(_gianHang.Ten))
            return "GH";

        var words = _gianHang.Ten
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(2)
            .Select(word => char.ToUpperInvariant(word[0]));

        return string.Concat(words);
    }

    private static string FormatPrice(decimal price)
    {
        return $"{price:N0}d";
    }

    private static ImageSource BuildImageSource(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return ImageSource.FromFile("dotnet_bot.png");

        if (Uri.TryCreate(imagePath, UriKind.Absolute, out var uri) &&
            (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return ImageSource.FromUri(uri);
        }

        return ImageSource.FromFile(imagePath);
    }
}
