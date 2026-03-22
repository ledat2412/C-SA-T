# 📁 Cấu trúc project MauiApp1

MauiApp1/
├── Main/                             # Thành phần khởi động và cấu hình ứng dụng
│   ├── App.xaml.cs                   # Khởi tạo App, thiết lập trang gốc
│   └── MauiProgram.cs                # Cấu hình MAUI builder, fonts, services
│
├── Models/                           # Các model dữ liệu dùng chung
│   ├── PoiItem.cs                    # Model điểm POI: tên, mô tả, tọa độ, ảnh...
│   ├── SheetState.cs                 # Trạng thái của bottom sheet (ẩn, nửa, toàn màn)
│   └── TaiKhoan.cs                   # Model tài khoản người dùng
│
├── Platforms/                        # Mã đặc thù theo từng nền tảng
│   ├── Android/                      # Cấu hình, permission, native code Android
│   ├── iOS/                          # Cấu hình native iOS
│   ├── Windows/                      # Cấu hình native Windows
│   └── MacCatalyst/                  # Cấu hình native Mac
│
├── Properties/                       # Thuộc tính build / launch của project
│   └── launchSettings.json           # Cấu hình chạy debug (nếu có)
│
├── Resources/                        # Tài nguyên giao diện và media
│   ├── Fonts/                        # Font chữ dùng trong app
│   ├── Images/                       # Ảnh icon, banner, ảnh POI demo
│   ├── AppIcon/                      # Icon ứng dụng
│   ├── Splash/                       # Màn hình splash
│   ├── Raw/                          # File tĩnh khác
│   └── Styles/                       # Màu sắc, style, theme dùng chung
│
├── Services/                         # Tầng xử lý dịch vụ / dữ liệu
│   └── MysqlServices.cs              # Kết nối MySQL, login, test DB
│
├── Views/                            # Tầng giao diện người dùng
│   ├── Auth/                         # Nhóm màn hình xác thực
│   │   ├── LoginPage.cs              # Màn hình đăng nhập
│   │   └── RegisterPage.cs           # Màn hình đăng ký
│   │
│   └── Maps/                         # Nhóm màn hình liên quan bản đồ / GPS / POI
│       ├── GpsPage.cs                # Demo lấy vị trí hiện tại
│       ├── MapGpsPage.cs             # Demo map kết hợp GPS và hiển thị vị trí
│       ├── OsmMapPage.cs             # Trang bản đồ OSM/Map chính kiểu cũ
│       └── PoiMapPage.cs             # Trang map POI + footer + bottom sheet khám phá
│
├── .gitignore                        # Khai báo file/thư mục không đẩy lên Git
└── MauiApp1.csproj                   # File project chính của .NET MAUI
