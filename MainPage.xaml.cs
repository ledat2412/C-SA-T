using MauiApp1.Services;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private readonly MySqlService _mySqlService;
        private bool _daKiemTra = false;

        public MainPage(MySqlService mySqlService)
        {
            InitializeComponent();
            _mySqlService = mySqlService;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (_daKiemTra) return;
            _daKiemTra = true;

            try
            {
                lblStatus.Text = "Đang kiểm tra kết nối...";

                bool ok = await _mySqlService.TestConnectionAsync();

                if (ok)
                {
                    lblStatus.Text = "Kết nối database thành công";
                    await DisplayAlert("Thông báo", "Kết nối database thành công", "OK");
                }
                else
                {
                    lblStatus.Text = "Kết nối database thất bại";
                    await DisplayAlert("Thông báo", "Kết nối database thất bại", "OK");
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Có lỗi khi kết nối database";
                await DisplayAlert("Lỗi", ex.ToString(), "OK");
            }
        }
    }
}