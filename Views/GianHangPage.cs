using MauiApp1.Models;
using MauiApp1.Services;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MauiApp1.Views
{
    public class GianHangPage : ContentPage
    {
        private readonly GianHangService _gianHangService;
        private readonly ObservableCollection<GianHang> _items = new();
        private readonly CollectionView _collectionView;

        public GianHangPage(GianHangService gianHangService)
        {
            _gianHangService = gianHangService;

            Title = "Danh sách gian hàng";

            var btnLoad = new Button
            {
                Text = "Tải dữ liệu gian hàng"
            };
            btnLoad.Clicked += OnLoadClicked;

            _collectionView = new CollectionView
            {
                ItemsSource = _items,
                ItemTemplate = new DataTemplate(() =>
                {
                    var diaChi = new Label { FontAttributes = FontAttributes.Bold };
                    diaChi.SetBinding(Label.TextProperty, "DiaChi");

                    var moTa = new Label();
                    moTa.SetBinding(Label.TextProperty, "MoTaChiNhanh");

                    var toaDo = new Label();
                    toaDo.SetBinding(Label.TextProperty,
                        new Binding(path: ".", converter: new GianHangToaDoConverter()));

                    return new VerticalStackLayout
                    {
                        Padding = 12,
                        Spacing = 4,
                        Children =
                        {
                            diaChi,
                            moTa,
                            toaDo
                        }
                    };
                })
            };

            Content = new VerticalStackLayout
            {
                Padding = 20,
                Spacing = 12,
                Children =
                {
                    btnLoad,
                    _collectionView
                }
            };
        }

        private async void OnLoadClicked(object? sender, EventArgs e)
        {
            _items.Clear();

            var data = await _gianHangService.GetAllAsync();

            foreach (var item in data)
                _items.Add(item);
        }
    }

    public class GianHangToaDoConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not GianHang gianHang)
                return "Không có dữ liệu";

            if (gianHang.Lat == null || gianHang.Lon == null)
                return "Chưa có tọa độ";

            return $"Lat: {gianHang.Lat}, Lon: {gianHang.Lon}";
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value ?? "";
        }
    }
}