using MauiApp1.Models;
using MauiApp1.Services;
using Microsoft.Maui.Controls.Shapes;

namespace MauiApp1.Views;

public class GianHangFoodGalleryPage : ContentPage
{
    private readonly GianHang _gianHang;
    private readonly MonAnService _monAnService;
    private readonly string _heroImage;

    private readonly VerticalStackLayout _popularList;
    private readonly HorizontalStackLayout _drinkRow;
    private readonly Border _featuredCard;

    private bool _isLoaded;

    public GianHangFoodGalleryPage(GianHang gianHang, MonAnService monAnService, string? heroImage)
    {
        _gianHang = gianHang;
        _monAnService = monAnService;
        _heroImage = string.IsNullOrWhiteSpace(heroImage) ? "dotnet_bot.png" : heroImage;

        NavigationPage.SetHasNavigationBar(this, false);
        BackgroundColor = Colors.White;

        _featuredCard = new Border
        {
            Stroke = Color.FromArgb("#EDEDED"),
            StrokeShape = new RoundRectangle { CornerRadius = 26 },
            Padding = 0,
            Content = CreateDishCard("Món nổi bật", "Mô tả đang cập nhật", 0, true)
        };

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
                    BuildCategoryChips(),
                    _featuredCard,
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
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_isLoaded)
            return;

        _isLoaded = true;
        await LoadMenuAsync();
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
            Text = "The Gastronomic\nGallery",
            FontSize = 30,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FF5A1F")
        };

        var avatar = new Border
        {
            HeightRequest = 34,
            WidthRequest = 34,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 17 },
            BackgroundColor = Color.FromArgb("#F1F1F1"),
            Content = new Image
            {
                Source = "dotnet_bot.png",
                Aspect = Aspect.AspectFill
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
            }
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
                    new Image { Source = _heroImage, Aspect = Aspect.AspectFill },
                    new Border
                    {
                        StrokeThickness = 0,
                        BackgroundColor = Color.FromRgba(0, 0, 0, 0.35),
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
                                        Text = "FINE DINING EXCELLENCE",
                                        FontSize = 11,
                                        FontAttributes = FontAttributes.Bold,
                                        TextColor = Colors.White
                                    }
                                },
                                new Label
                                {
                                    Text = string.IsNullOrWhiteSpace(_gianHang.Ten) ? "Nhà hàng" : _gianHang.Ten,
                                    FontSize = 50,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = Colors.White
                                },
                                new Label
                                {
                                    Text = "⭐ 4.9 (2.4k reviews)  ·  ⏰ 07:00 - 22:00",
                                    FontSize = 12,
                                    TextColor = Colors.White
                                }
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

    private View BuildSectionTitle(string title, string action)
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
            FontSize = 32,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#1F1F1F")
        });

        var right = new Label
        {
            Text = action,
            FontSize = 12,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FF6B00"),
            VerticalTextAlignment = TextAlignment.End
        };

        grid.Children.Add(right);
        Grid.SetColumn(right, 1);

        return grid;
    }

    private async Task LoadMenuAsync()
    {
        var items = await _monAnService.GetByChiNhanhAsync(_gianHang.IdGianHang);
        if (items.Count == 0)
        {
            items = await _monAnService.GetAllAsync();
        }

        var featured = items.FirstOrDefault();
        if (featured is not null)
        {
            _featuredCard.Content = CreateDishCard(featured.TenMon, featured.ThongTinMon, featured.DonGia, true);
        }

        _popularList.Children.Clear();
        foreach (var item in items.Skip(1).Take(3))
        {
            _popularList.Children.Add(CreateDishCard(item.TenMon, item.ThongTinMon, item.DonGia, false));
        }

        _drinkRow.Children.Clear();
        foreach (var item in items.Take(4))
        {
            _drinkRow.Children.Add(CreateDrinkCard(item.TenMon, item.DonGia));
        }
    }

    private View CreateDishCard(string? name, string? description, decimal price, bool large)
    {
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
                            Source = "dotnet_bot.png",
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

    private View CreateDrinkCard(string? name, decimal price)
    {
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
                            Source = "dotnet_bot.png",
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
}
