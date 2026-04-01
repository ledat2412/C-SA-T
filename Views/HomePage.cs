using Microsoft.Maui.Controls.Shapes;

namespace MauiApp1.Views;

public class HomePage : ContentPage
{
    private readonly Label _dateLabel;

    public HomePage()
    {
        Title = "Trang chủ";
        BackgroundColor = Color.FromArgb("#F3F3F6");

        _dateLabel = new Label
        {
            FontSize = 12,
            TextColor = Color.FromArgb("#8E8E93"),
            CharacterSpacing = 1
        };

        var root = new Grid
        {
            RowDefinitions =
            {
                new RowDefinition(GridLength.Star),
                new RowDefinition(GridLength.Auto)
            },
            Padding = new Thickness(12, 12, 12, 0)
        };

        var scroll = new ScrollView
        {
            Content = new VerticalStackLayout
            {
                Spacing = 22,
                Padding = new Thickness(14, 10, 14, 20),
                Children =
                {
                    BuildTopRow(),
                    BuildGreetingSection(),
                    BuildSearchRow(),
                    BuildExploreSection(),
                    BuildNearbySection(),
                    BuildPromoCard()
                }
            }
        };

        var body = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            Content = scroll,
            Margin = new Thickness(0, 0, 0, 0)
        };

        root.Children.Add(body);

        var footer = BuildFooter();
        root.Children.Add(footer);
        Grid.SetRow(footer, 1);

        Content = root;

        UpdateDateLabel();
        Dispatcher.StartTimer(TimeSpan.FromMinutes(1), () =>
        {
            UpdateDateLabel();
            return true;
        });
    }

    private void UpdateDateLabel()
    {
        var now = DateTime.Now;

        var dayText = now.DayOfWeek switch
        {
            DayOfWeek.Monday => "THỨ HAI",
            DayOfWeek.Tuesday => "THỨ BA",
            DayOfWeek.Wednesday => "THỨ TƯ",
            DayOfWeek.Thursday => "THỨ NĂM",
            DayOfWeek.Friday => "THỨ SÁU",
            DayOfWeek.Saturday => "THỨ BẢY",
            _ => "CHỦ NHẬT"
        };

        _dateLabel.Text = $"{dayText}, {now:dd} THÁNG {now.Month}";
    }

    private View BuildTopRow()
    {
        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 8
        };

        grid.Children.Add(new Label
        {
            Text = "📍",
            FontSize = 14,
            TextColor = Color.FromArgb("#FF6B00"),
            VerticalTextAlignment = TextAlignment.Center
        });

        var location = new Label
        {
            Text = "Ho Chi MinhCity,District1",
            FontSize = 20,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#30343F"),
            VerticalTextAlignment = TextAlignment.Center
        };
        grid.Children.Add(location);
        Grid.SetColumn(location, 1);

        var avatar = new Border
        {
            HeightRequest = 42,
            WidthRequest = 42,
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 21 },
            BackgroundColor = Color.FromArgb("#F1F1F1"),
            Content = new Image
            {
                Source = "dotnet_bot.png",
                Aspect = Aspect.AspectFill
            }
        };
        grid.Children.Add(avatar);
        Grid.SetColumn(avatar, 2);

        return grid;
    }

    private View BuildGreetingSection()
    {
        return new VerticalStackLayout
        {
            Spacing = 4,
            Children =
            {
                _dateLabel,
                //new Label
                //{
                //    Text = "Chào buổi sáng,",
                //    FontSize = 28,
                //    FontAttributes = FontAttributes.Bold,
                //    TextColor = Color.FromArgb("#1F1F23")
                //},
                //new Label
                //{
                //    Text = "User!",
                //    FontSize = 30,
                //    FontAttributes = FontAttributes.Bold | FontAttributes.Italic,
                //    TextColor = Color.FromArgb("#FF6B00")
                //}
            }
        };
    }

    private View BuildSearchRow()
    {
        var row = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            },
            ColumnSpacing = 12
        };

        var searchContent = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star)
            },
            ColumnSpacing = 10
        };

        var searchIcon = new Label
        {
            Text = "🔍",
            FontSize = 18,
            TextColor = Color.FromArgb("#777777"),
            VerticalTextAlignment = TextAlignment.Center
        };

        var searchText = new Label
        {
            Text = "Tìm kiếm món ăn, nhà hàng",
            FontSize = 16,
            TextColor = Color.FromArgb("#8B8B8B"),
            VerticalTextAlignment = TextAlignment.Center
        };

        searchContent.Children.Add(searchIcon);
        searchContent.Children.Add(searchText);
        Grid.SetColumn(searchText, 1);

        var searchBox = new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Color.FromArgb("#F2F2F2"),
            StrokeShape = new RoundRectangle { CornerRadius = 22 },
            Padding = new Thickness(16, 14),
            Content = searchContent
        };

        row.Children.Add(searchBox);
        return row;
    }

    private View BuildExploreSection()
    {
        var section = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                BuildSectionHeader("Khám phá", "Xem tất cả")
            }
        };

        var cards = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1.15, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star))
            },
            ColumnSpacing = 12,
            HeightRequest = 170
        };

        cards.Children.Add(new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Content = new Grid
            {
                Children =
                {
                    new Image { Source = "dotnet_bot.png", Aspect = Aspect.AspectFill },
                    new Border
                    {
                        StrokeThickness = 0,
                        BackgroundColor = Color.FromRgba(0,0,0,0.4),
                        StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(0,0,16,16) },
                        VerticalOptions = LayoutOptions.End,
                        Padding = new Thickness(10,6),
                        Content = new Label
                        {
                            Text = "Hẹn hò",
                            FontSize = 18,
                            FontAttributes = FontAttributes.Bold,
                            TextColor = Colors.White
                        }
                    }
                }
            }
        });

        var right = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                BuildTagCard("🔥", "Phổ biến", false),
                BuildTagCard("%", "Ưu đãi", true)
            }
        };

        cards.Children.Add(right);
        Grid.SetColumn(right, 1);

        section.Children.Add(cards);
        return section;
    }

    private View BuildNearbySection()
    {
        var section = new VerticalStackLayout
        {
            Spacing = 12,
            Children =
            {
                BuildSectionHeader("Các quán ăn gần đây", "Gần nhất")
            }
        };

        var row = new HorizontalStackLayout
        {
            Spacing = 12,
            Children =
            {
                BuildRestaurantCard("Noir. Dining in the Dark", "0.8 km · $$$$", "4.8"),
                BuildRestaurantCard("Quin House", "1.2 km · $$$", "4.6")
            }
        };

        section.Children.Add(new ScrollView
        {
            Orientation = ScrollOrientation.Horizontal,
            Content = row
        });

        return section;
    }

    private View BuildRestaurantCard(string title, string meta, string rating)
    {
        return new Border
        {
            Stroke = Color.FromArgb("#EFEFEF"),
            StrokeThickness = 1,
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = 18 },
            WidthRequest = 255,
            Padding = new Thickness(10),
            Content = new VerticalStackLayout
            {
                Spacing = 8,
                Children =
                {
                    new Border
                    {
                        StrokeThickness = 0,
                        HeightRequest = 130,
                        StrokeShape = new RoundRectangle { CornerRadius = 14 },
                        Content = new Grid
                        {
                            Children =
                            {
                                new Image { Source = "dotnet_bot.png", Aspect = Aspect.AspectFill },
                                new Border
                                {
                                    StrokeThickness = 0,
                                    Padding = new Thickness(10,2),
                                    BackgroundColor = Colors.White,
                                    StrokeShape = new RoundRectangle { CornerRadius = 12 },
                                    HorizontalOptions = LayoutOptions.End,
                                    VerticalOptions = LayoutOptions.Start,
                                    Margin = new Thickness(8),
                                    Content = new Label
                                    {
                                        Text = $"⭐ {rating}",
                                        FontSize = 12,
                                        FontAttributes = FontAttributes.Bold,
                                        TextColor = Color.FromArgb("#444")
                                    }
                                }
                            }
                        }
                    },
                    new Label
                    {
                        Text = title,
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#262626")
                    },
                    new Label
                    {
                        Text = $"📍 {meta}",
                        FontSize = 13,
                        TextColor = Color.FromArgb("#666")
                    }
                }
            }
        };
    }

    private View BuildPromoCard()
    {
        return new Border
        {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 24 },
            BackgroundColor = Color.FromArgb("#FFF3EA"),
            Padding = new Thickness(16),
            Margin = new Thickness(0, 4, 0, 4),
            Content = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(GridLength.Star),
                    new ColumnDefinition(GridLength.Auto)
                },
                Children =
                {
                    new VerticalStackLayout
                    {
                        Spacing = 10,
                        Children =
                        {
                            new Label
                            {
                                Text = "Khám phá\nhương vị bản\nđịa",
                                FontSize = 38,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.FromArgb("#2F2F33")
                            },
                            new Label
                            {
                                Text = "Được tuyển chọn bởi các chuyên gia ẩm thực hàng đầu tại Sài Gòn.",
                                FontSize = 14,
                                TextColor = Color.FromArgb("#6D6D72")
                            },
                            new Border
                            {
                                StrokeThickness = 0,
                                BackgroundColor = Color.FromArgb("#FF6B00"),
                                StrokeShape = new RoundRectangle { CornerRadius = 20 },
                                Padding = new Thickness(18, 10),
                                HorizontalOptions = LayoutOptions.Start,
                                Content = new Label
                                {
                                    Text = "Thử ngay",
                                    FontSize = 14,
                                    FontAttributes = FontAttributes.Bold,
                                    TextColor = Colors.White
                                }
                            }
                        }
                    },
                    new Border
                    {
                        HeightRequest = 118,
                        WidthRequest = 118,
                        BackgroundColor = Colors.White,
                        StrokeThickness = 0,
                        StrokeShape = new RoundRectangle { CornerRadius = 59 },
                        HorizontalOptions = LayoutOptions.End,
                        VerticalOptions = LayoutOptions.End,
                        Content = new Image
                        {
                            Source = "dotnet_bot.png",
                            Aspect = Aspect.AspectFill
                        }
                    }
                }
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

        grid.Children.Add(new Label
        {
            Text = title,
            FontSize = 37,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#222")
        });

        var actionLabel = new Label
        {
            Text = action,
            FontSize = 14,
            FontAttributes = FontAttributes.Bold,
            TextColor = Color.FromArgb("#FF7A1A"),
            VerticalTextAlignment = TextAlignment.End
        };
        grid.Children.Add(actionLabel);
        Grid.SetColumn(actionLabel, 1);

        return grid;
    }

    private View BuildTagCard(string icon, string title, bool outlined)
    {
        return new Border
        {
            Stroke = outlined ? Color.FromArgb("#FFD9C2") : Colors.Transparent,
            StrokeThickness = outlined ? 1 : 0,
            BackgroundColor = outlined ? Color.FromArgb("#FFF7F1") : Color.FromArgb("#F2F2F2"),
            StrokeShape = new RoundRectangle { CornerRadius = 16 },
            Padding = new Thickness(14, 16),
            Content = new HorizontalStackLayout
            {
                Spacing = 8,
                HorizontalOptions = LayoutOptions.Center,
                Children =
                {
                    new Label
                    {
                        Text = icon,
                        FontSize = 18,
                        TextColor = Color.FromArgb("#A16207")
                    },
                    new Label
                    {
                        Text = title,
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = Color.FromArgb("#454545")
                    }
                }
            }
        };
    }

    private View BuildFooter()
    {
        View BuildFooterItem(string icon, string text, bool active, Func<Task>? onTap = null)
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
                        BackgroundColor = active ? Color.FromArgb("#FFEBDD") : Colors.Transparent,
                        StrokeShape = new RoundRectangle { CornerRadius = 18 },
                        Padding = new Thickness(12, 8),
                        Content = new Label
                        {
                            Text = icon,
                            FontSize = 18,
                            HorizontalTextAlignment = TextAlignment.Center
                        }
                    },
                    new Label
                    {
                        Text = text,
                        FontSize = 11,
                        FontAttributes = FontAttributes.Bold,
                        TextColor = active ? Color.FromArgb("#FF7A1A") : Color.FromArgb("#7D7D82"),
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

        var tabs = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Star)
            },
            Padding = new Thickness(16, 8, 16, 12)
        };

        var discover = BuildFooterItem("🧭", "DISCOVER", true);
        var search = BuildFooterItem("🔎", "SEARCH", false, async () => await Navigation.PopAsync());
        var orders = BuildFooterItem("🍴", "ORDERS", false);
        var profile = BuildFooterItem("👤", "PROFILE", false);

        tabs.Children.Add(discover);
        tabs.Children.Add(search);
        Grid.SetColumn(search, 1);
        tabs.Children.Add(orders);
        Grid.SetColumn(orders, 2);
        tabs.Children.Add(profile);
        Grid.SetColumn(profile, 3);

        return new Border
        {
            StrokeThickness = 0,
            BackgroundColor = Colors.White,
            StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(24, 24, 0, 0) },
            Content = tabs,
            Margin = new Thickness(0, 0, 0, 0),
            Shadow = new Shadow
            {
                Brush = Brush.Black,
                Opacity = 0.08f,
                Radius = 14,
                Offset = new Point(0, -2)
            }
        };
    }
}
