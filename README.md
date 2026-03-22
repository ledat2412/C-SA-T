# 📁 Cấu trúc project MauiApp1

```
# 📁 Cấu trúc project MauiApp1

MauiApp1/
├── Main/                              # Thành phần khởi động và cấu hình ứng dụng
│   ├── App.xaml.cs                    # Khởi tạo App, thiết lập trang gốc
│   └── MauiProgram.cs                 # Cấu hình MAUI builder, fonts, services
│
├── Models/                            # Các model dữ liệu dùng chung
│   ├── PoiItem.cs                     # Model điểm POI (tên, tọa độ, ảnh...)
│   ├── SheetState.cs                  # Trạng thái bottom sheet
│   └── TaiKhoan.cs                    # Model tài khoản người dùng
│
├── Platforms/                         # Mã đặc thù theo từng nền tảng
│   ├── Android/                       # Cấu hình Android
│   ├── iOS/                           # Cấu hình iOS
│   ├── Windows/                       # Cấu hình Windows
│   └── MacCatalyst/                   # Cấu hình Mac
│
├── Properties/                        # Thuộc tính build / launch
│   └── launchSettings.json            # Cấu hình debug
│
├── Resources/                         # Tài nguyên giao diện
│   ├── Fonts/                         # Font chữ
│   ├── Images/                        # Ảnh, icon, banner
│   ├── AppIcon/                       # Icon app
│   ├── Splash/                        # Splash screen
│   ├── Raw/                           # File tĩnh khác
│   └── Styles/                        # Theme, màu sắc
│
├── Services/                          # Tầng xử lý dữ liệu
│   └── MysqlServices.cs               # Kết nối MySQL, login
│
├── Views/                             # Tầng giao diện
│   ├── Auth/                          # Xác thực
│   │   ├── LoginPage.cs               # Đăng nhập
│   │   └── RegisterPage.cs            # Đăng ký
│   │
│   └── Maps/                          # Bản đồ / GPS / POI
│       ├── GpsPage.cs                 # Demo GPS
│       ├── MapGpsPage.cs              # Map + GPS
│       ├── OsmMapPage.cs              # Map cơ bản
│       └── PoiMapPage.cs              # Map chính + khám phá
│
├── .gitignore                         # Bỏ qua file khi push
└── MauiApp1.csproj                    # File project chính
```
