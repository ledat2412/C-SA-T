# ĐẶC TẢ YÊU CẦU: HỆ THỐNG XEM TRƯỚC GIAN HÀNG TRÊN BẢN ĐỒ & THUYẾT MINH TTS

## 1. Tổng quan dự án

Xây dựng hệ thống đa nền tảng hỗ trợ người dùng khám phá các gian hàng ẩm thực trên bản đồ, xem trước thông tin gian hàng, nghe mô tả bằng **Text-to-Speech (TTS)** và tra cứu menu/món ăn trước khi đến trực tiếp. Ngoài ra, hệ thống còn cung cấp website quản trị cho chủ gian hàng, nhân viên và quản trị viên để quản lý gian hàng, món ăn, tài khoản và báo cáo cơ bản.

Hệ thống tập trung vào 3 giá trị chính:
- **Khám phá trực quan**: xem gian hàng trực tiếp trên bản đồ.
- **Tiếp cận thuận tiện**: nghe mô tả gian hàng/món ăn bằng giọng nói tiếng Việt.
- **Quản trị tập trung**: hỗ trợ quản lý dữ liệu gian hàng, món ăn, nhân sự và hóa đơn.

### 1.1 Mục tiêu sản phẩm
- Giúp khách hàng tìm thấy gian hàng gần vị trí hiện tại.
- Cho phép xem nhanh thông tin gian hàng ngay trên bản đồ.
- Hỗ trợ nghe nội dung mô tả bằng âm thanh sinh từ văn bản.
- Giúp quản lý cập nhật menu, món ăn và thông tin gian hàng dễ dàng.
- Cung cấp nền tảng quản lý cho admin để duyệt gian hàng và giám sát hoạt động hệ thống.

### 1.2 Đối tượng sử dụng
- **Khách hàng**
- **Nhân viên**
- **Quản lý gian hàng**
- **Quản trị viên**

---

## 2. Yêu cầu chức năng chi tiết

### 2.1 Quản lý bản đồ và xem trước gian hàng
- Hiển thị danh sách gian hàng dưới dạng marker trên bản đồ.
- Người dùng có thể chạm vào marker để mở **preview card**.
- Preview card hiển thị:
  - Tên gian hàng
  - Địa chỉ
  - Mô tả ngắn
  - Trạng thái hoạt động
  - Nút xem chi tiết
  - Nút nghe mô tả
- Hệ thống có thể lọc gian hàng theo:
  - Khoảng cách gần người dùng
  - Trạng thái hoạt động
  - Từ khóa tìm kiếm

### 2.2 Quản lý vị trí và GPS
- Hệ thống lấy vị trí GPS hiện tại của người dùng.
- Cho phép cập nhật vị trí người dùng theo thời gian thực hoặc theo chu kỳ.
- Tính khoảng cách từ người dùng đến từng gian hàng.
- Hỗ trợ đề xuất gian hàng gần nhất.

### 2.3 Quản lý Text-to-Speech (TTS)
- Người dùng có thể nghe mô tả gian hàng.
- Người dùng có thể nghe mô tả món ăn.
- Hệ thống hỗ trợ các thao tác:
  - Phát
  - Tạm dừng
  - Dừng
  - Phát lại
- Nội dung audio được sinh từ văn bản mô tả đã lưu trong hệ thống.
- Có thể mở rộng để dùng audio ghi âm sẵn hoặc audio sinh tự động.

### 2.4 Quản lý gian hàng
- Chủ gian hàng/quản lý có thể:
  - Tạo gian hàng
  - Cập nhật thông tin gian hàng
  - Cập nhật tọa độ và vị trí bản đồ
  - Cập nhật mô tả gian hàng
  - Bật/tắt trạng thái hoạt động
- Admin có thể:
  - Xem toàn bộ danh sách gian hàng
  - Duyệt hoặc từ chối gian hàng
  - Khóa/mở hiển thị gian hàng

### 2.5 Quản lý menu và món ăn
- Mỗi gian hàng có nhiều món ăn.
- Chủ gian hàng có thể:
  - Thêm món ăn
  - Sửa món ăn
  - Cập nhật giá
  - Cập nhật trạng thái còn bán / tạm hết / ngừng bán
  - Thêm hình ảnh món ăn
  - Cập nhật mô tả món ăn
- Khách hàng có thể:
  - Xem danh sách món ăn theo gian hàng
  - Xem chi tiết món ăn
  - Nghe mô tả món ăn bằng TTS

### 2.6 Quản lý tài khoản
- Đăng ký tài khoản bằng email và mật khẩu.
- Đăng nhập, đăng xuất.
- Phân quyền theo vai trò:
  - Khách hàng
  - Nhân viên
  - Quản lý gian hàng
  - Quản trị viên
- Quản trị viên có quyền khóa/mở tài khoản.

### 2.7 Quản lý hóa đơn
- Nhân viên có thể tạo hóa đơn.
- Hóa đơn gồm thông tin khách hàng, nhân viên lập, danh sách món, ghi chú, tổng tiền, khuyến mãi và trạng thái.
- Mỗi hóa đơn có nhiều chi tiết hóa đơn.
- Có thể mở rộng để thống kê doanh thu theo gian hàng.

### 2.8 Website dành cho chủ gian hàng và admin
- **Chủ gian hàng**:
  - Quản lý gian hàng
  - Quản lý món ăn
  - Xem danh sách hóa đơn
  - Quản lý nhân viên phụ trách
- **Admin**:
  - Quản lý tài khoản
  - Duyệt gian hàng
  - Xem báo cáo doanh thu cơ bản
  - Quản lý dữ liệu toàn hệ thống

---

## 3. Yêu cầu phi chức năng

### 3.1 Hiệu năng
- Danh sách gian hàng trên bản đồ phản hồi nhanh dưới 3 giây trong điều kiện mạng ổn định.
- Khi bấm vào marker, preview card hiển thị gần như tức thời.

### 3.2 Bảo mật
- Mật khẩu được mã hóa.
- API yêu cầu xác thực bằng token.
- Phân quyền rõ ràng theo vai trò.

### 3.3 Khả năng mở rộng
- Hỗ trợ mở rộng thêm đặt món online, đánh giá gian hàng, chỉ đường, thông báo khuyến mãi, đa ngôn ngữ.

### 3.4 Tương thích
- Hỗ trợ ứng dụng mobile và web admin.
- Có thể tích hợp Google Maps hoặc OpenStreetMap.

---

## 4. Use case chính

### 4.1 Khách hàng
- Đăng ký tài khoản
- Đăng nhập
- Tìm kiếm gian hàng
- Xem gian hàng trên bản đồ
- Xem preview thông tin gian hàng
- Xem menu và món ăn
- Nghe mô tả gian hàng
- Nghe mô tả món ăn
- Xem vị trí gian hàng

### 4.2 Nhân viên / Quản lý gian hàng
- Đăng nhập quản trị
- Quản lý thông tin gian hàng
- Quản lý menu / món ăn
- Tạo hóa đơn
- Quản lý nhân viên phụ trách

### 4.3 Quản trị viên
- Quản lý tài khoản
- Duyệt gian hàng
- Quản lý toàn bộ dữ liệu hệ thống
- Xem thống kê doanh thu / thu nhập cơ bản

---

## 5. Data Pipeline / Luồng dữ liệu

### 5.1 Luồng xem trước gian hàng trên bản đồ
1. Người dùng mở ứng dụng.
2. Hệ thống lấy vị trí hiện tại từ GPS.
3. Hệ thống tải danh sách gian hàng gần vị trí người dùng.
4. Bản đồ hiển thị marker gian hàng.
5. Người dùng chọn một marker.
6. Hệ thống hiển thị preview card gồm tên, địa chỉ, mô tả, trạng thái và nút nghe audio.
7. Người dùng có thể chuyển sang trang chi tiết để xem menu và món ăn.

### 5.2 Luồng Text-to-Speech
1. Người dùng nhấn nút nghe mô tả.
2. Hệ thống lấy nội dung mô tả dạng văn bản.
3. Hệ thống sinh audio hoặc lấy file audio đã tồn tại.
4. Ứng dụng phát audio cho người dùng.
5. Người dùng có thể tạm dừng, dừng hoặc phát lại.

### 5.3 Luồng quản lý dữ liệu
1. Chủ gian hàng/admin đăng nhập web quản trị.
2. Chỉnh sửa gian hàng hoặc món ăn.
3. Hệ thống ghi dữ liệu vào database.
4. Ứng dụng mobile đồng bộ và hiển thị dữ liệu mới.

---

## 6. Database Schemas

### 6.1 Bảng `tai_khoan`
```sql
CREATE TABLE tai_khoan (
    id_tai_khoan INT PRIMARY KEY AUTO_INCREMENT,
    email VARCHAR(100) NOT NULL UNIQUE,
    mat_khau VARCHAR(255) NOT NULL,
    ten_nguoi_dung VARCHAR(100) NOT NULL,
    vai_tro ENUM('KHACH_HANG', 'NHAN_VIEN', 'QUAN_LY_GIAN_HANG', 'QUAN_TRI_VIEN') NOT NULL,
    tinh_trang ENUM('CHO_DUYET', 'HOAT_DONG', 'BI_KHOA') DEFAULT 'CHO_DUYET',
    thoi_gian_dang_ky DATETIME DEFAULT CURRENT_TIMESTAMP,
    cap_nhat_luc DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);
```

### 6.2 Bảng `khach_hang`
```sql
CREATE TABLE khach_hang (
    id_khach_hang INT PRIMARY KEY AUTO_INCREMENT,
    id_tai_khoan INT NOT NULL UNIQUE,
    so_dien_thoai VARCHAR(20),
    FOREIGN KEY (id_tai_khoan) REFERENCES tai_khoan(id_tai_khoan)
);
```

### 6.3 Bảng `nhan_vien`
```sql
CREATE TABLE nhan_vien (
    id_nhan_vien INT PRIMARY KEY AUTO_INCREMENT,
    id_tai_khoan INT NOT NULL UNIQUE,
    so_dien_thoai VARCHAR(20),
    FOREIGN KEY (id_tai_khoan) REFERENCES tai_khoan(id_tai_khoan)
);
```

### 6.4 Bảng `gian_hang`
```sql
CREATE TABLE gian_hang (
    id_gian_hang INT PRIMARY KEY AUTO_INCREMENT,
    ten_gian_hang VARCHAR(150) NOT NULL,
    dia_chi VARCHAR(255) NOT NULL,
    vi_do DECIMAL(10,7) NOT NULL,
    kinh_do DECIMAL(10,7) NOT NULL,
    mo_ta TEXT,
    tinh_trang ENUM('CHO_DUYET', 'DANG_HOAT_DONG', 'TAM_NGUNG', 'DONG_CUA') DEFAULT 'CHO_DUYET',
    phi_hang_thang DECIMAL(12,2) DEFAULT 0,
    ngay_dang_ky DATETIME DEFAULT CURRENT_TIMESTAMP,
    thoi_gian_cap_nhat DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    duoc_duyet BOOLEAN DEFAULT FALSE
);
```

### 6.5 Bảng `phu_trach_gian_hang`
```sql
CREATE TABLE phu_trach_gian_hang (
    id INT PRIMARY KEY AUTO_INCREMENT,
    id_nhan_vien INT NOT NULL,
    id_gian_hang INT NOT NULL,
    vai_tro_phu_trach ENUM('NHAN_VIEN', 'QUAN_LY') DEFAULT 'NHAN_VIEN',
    ngay_phan_cong DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (id_nhan_vien, id_gian_hang),
    FOREIGN KEY (id_nhan_vien) REFERENCES nhan_vien(id_nhan_vien),
    FOREIGN KEY (id_gian_hang) REFERENCES gian_hang(id_gian_hang)
);
```

### 6.6 Bảng `mon_an`
```sql
CREATE TABLE mon_an (
    id_mon_an INT PRIMARY KEY AUTO_INCREMENT,
    id_gian_hang INT NOT NULL,
    ten_mon VARCHAR(150) NOT NULL,
    thong_tin_mon TEXT,
    don_gia DECIMAL(12,2) NOT NULL,
    hinh_anh VARCHAR(255),
    tinh_trang ENUM('CON_BAN', 'TAM_HET', 'NGUNG_BAN') DEFAULT 'CON_BAN',
    thoi_gian_cap_nhat DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (id_gian_hang) REFERENCES gian_hang(id_gian_hang)
);
```

### 6.7 Bảng `hoa_don`
```sql
CREATE TABLE hoa_don (
    id_hoa_don INT PRIMARY KEY AUTO_INCREMENT,
    id_khach_hang INT NOT NULL,
    id_nhan_vien INT NOT NULL,
    tong_tien DECIMAL(12,2) NOT NULL DEFAULT 0,
    khuyen_mai DECIMAL(12,2) DEFAULT 0,
    ghi_chu TEXT,
    tinh_trang ENUM('MOI_TAO', 'DA_THANH_TOAN', 'DA_HUY') DEFAULT 'MOI_TAO',
    thoi_gian_tao DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (id_khach_hang) REFERENCES khach_hang(id_khach_hang),
    FOREIGN KEY (id_nhan_vien) REFERENCES nhan_vien(id_nhan_vien)
);
```

### 6.8 Bảng `chi_tiet_hoa_don`
```sql
CREATE TABLE chi_tiet_hoa_don (
    id INT PRIMARY KEY AUTO_INCREMENT,
    id_hoa_don INT NOT NULL,
    id_mon_an INT NOT NULL,
    so_luong INT NOT NULL,
    don_gia DECIMAL(12,2) NOT NULL,
    ghi_chu TEXT,
    FOREIGN KEY (id_hoa_don) REFERENCES hoa_don(id_hoa_don),
    FOREIGN KEY (id_mon_an) REFERENCES mon_an(id_mon_an)
);
```

### 6.9 Bảng `lich_su_vi_tri`
```sql
CREATE TABLE lich_su_vi_tri (
    id INT PRIMARY KEY AUTO_INCREMENT,
    id_gian_hang INT NOT NULL,
    vi_do DECIMAL(10,7) NOT NULL,
    kinh_do DECIMAL(10,7) NOT NULL,
    thoi_gian_cap_nhat DATETIME DEFAULT CURRENT_TIMESTAMP,
    nguon_cap_nhat ENUM('GPS', 'THU_CONG', 'HE_THONG') DEFAULT 'GPS',
    FOREIGN KEY (id_gian_hang) REFERENCES gian_hang(id_gian_hang)
);
```

### 6.10 Bảng `audio_mo_ta`
```sql
CREATE TABLE audio_mo_ta (
    id INT PRIMARY KEY AUTO_INCREMENT,
    loai_doi_tuong ENUM('GIAN_HANG', 'MON_AN') NOT NULL,
    id_doi_tuong INT NOT NULL,
    noi_dung_van_ban TEXT NOT NULL,
    duong_dan_audio VARCHAR(255),
    ngon_ngu VARCHAR(10) DEFAULT 'vi-VN',
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

---

## 7. Class Design

### 7.1 `TaiKhoan`
```csharp
public class TaiKhoan
{
    public int IdTaiKhoan { get; set; }
    public string Email { get; set; }
    public string MatKhau { get; set; }
    public string TenNguoiDung { get; set; }
    public string VaiTro { get; set; }
    public string TinhTrang { get; set; }
    public DateTime ThoiGianDangKy { get; set; }

    public bool DangNhap(string email, string matKhau) { return true; }
    public bool DangKy() { return true; }
    public void DangXuat() { }
    public void DoiMatKhau(string matKhauMoi) { }
    public void CapNhatThongTin() { }
}
```

### 7.2 `KhachHang`
```csharp
public class KhachHang
{
    public int IdKhachHang { get; set; }
    public int IdTaiKhoan { get; set; }
    public string SoDienThoai { get; set; }

    public List<HoaDon> LayDanhSachHoaDon() => new();
}
```

### 7.3 `NhanVien`
```csharp
public class NhanVien
{
    public int IdNhanVien { get; set; }
    public int IdTaiKhoan { get; set; }
    public string SoDienThoai { get; set; }

    public HoaDon TaoHoaDon() => new HoaDon();
    public void CapNhatTinhTrang() { }
}
```

### 7.4 `GianHang`
```csharp
public class GianHang
{
    public int IdGianHang { get; set; }
    public string TenGianHang { get; set; }
    public string DiaChi { get; set; }
    public double ViDo { get; set; }
    public double KinhDo { get; set; }
    public string MoTa { get; set; }
    public string TinhTrang { get; set; }
    public decimal PhiHangThang { get; set; }
    public DateTime NgayDangKy { get; set; }
    public DateTime ThoiGianCapNhat { get; set; }

    public string TaoNoiDungPreview() => $"{TenGianHang} - {DiaChi} - {MoTa}";
}
```

### 7.5 `MonAn`
```csharp
public class MonAn
{
    public int IdMonAn { get; set; }
    public int IdGianHang { get; set; }
    public string Ten { get; set; }
    public string ThongTinMon { get; set; }
    public decimal DonGia { get; set; }
    public string HinhAnh { get; set; }
    public string TinhTrang { get; set; }
}
```

### 7.6 `HoaDon`
```csharp
public class HoaDon
{
    public int IdHoaDon { get; set; }
    public int IdKhachHang { get; set; }
    public int IdNhanVien { get; set; }
    public decimal TongTien { get; set; }
    public decimal KhuyenMai { get; set; }
    public string GhiChu { get; set; }
    public string TinhTrang { get; set; }
    public DateTime ThoiGianTao { get; set; }

    public List<ChiTietHoaDon> DanhSachChiTiet { get; set; } = new();
}
```

### 7.7 `ChiTietHoaDon`
```csharp
public class ChiTietHoaDon
{
    public int Id { get; set; }
    public int IdHoaDon { get; set; }
    public int IdMonAn { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public string GhiChu { get; set; }
}
```

### 7.8 `TextToSpeechService`
```csharp
public class TextToSpeechService
{
    public string TaoAudioTuVanBan(string noiDung, string ngonNgu = "vi-VN")
    {
        return "audio_url_or_path";
    }

    public void PhatAudio(string audioPath) { }
    public void TamDungAudio() { }
    public void DungAudio() { }
}
```

### 7.9 `MapService`
```csharp
public class MapService
{
    public List<GianHang> LayDanhSachGianHangGanDay(double viDo, double kinhDo, double banKinhKm)
    {
        return new List<GianHang>();
    }

    public GianHang LayChiTietGianHang(int idGianHang)
    {
        return new GianHang();
    }
}
```

---

## 8. API gợi ý

### Auth
- `POST /api/auth/register`
- `POST /api/auth/login`

### Gian hàng
- `GET /api/stalls`
- `GET /api/stalls/{id}`
- `GET /api/stalls/nearby?lat=...&lng=...`
- `POST /api/stalls`
- `PUT /api/stalls/{id}`

### Món ăn
- `GET /api/stalls/{id}/foods`
- `GET /api/foods/{id}`
- `POST /api/foods`
- `PUT /api/foods/{id}`

### Hóa đơn
- `POST /api/invoices`
- `GET /api/invoices/{id}`

### TTS
- `POST /api/tts/stall/{id}`
- `POST /api/tts/food/{id}`

### Bản đồ / vị trí
- `GET /api/map/markers`
- `POST /api/location/update`

---

## 9. Tính năng tham khảo mở rộng

### 9.1 Tính năng nên có sau MVP
- Chỉ đường đến gian hàng
- Đánh giá và bình luận gian hàng
- Lưu gian hàng yêu thích
- Thông báo khuyến mãi
- Đặt món trước
- QR check-in tại gian hàng
- Đa ngôn ngữ Việt / Anh
- Gợi ý gian hàng gần nhất theo hành vi người dùng
- Chọn giọng đọc nam / nữ cho TTS
- Tự động đọc mô tả khi mở preview

### 9.2 Tính năng phân tích dữ liệu
- Thống kê lượt xem gian hàng
- Thống kê lượt nghe audio
- Thống kê món ăn được quan tâm nhiều nhất
- Heatmap khu vực truy cập nhiều

---

## 10. Phạm vi MVP đề xuất

Phiên bản MVP nên bao gồm:
- Đăng ký / đăng nhập
- Xem gian hàng trên bản đồ
- Xem preview gian hàng
- Xem menu và món ăn
- Nghe mô tả bằng TTS
- Quản lý gian hàng cơ bản
- Quản lý món ăn cơ bản

Chưa cần làm ngay ở MVP:
- Đặt món online hoàn chỉnh
- Đánh giá sao
- Gợi ý AI nâng cao
- Báo cáo phân tích chuyên sâu

---

## 11. Kết luận

Tài liệu này mô tả hệ thống theo hướng có thể dùng cho:
- README dự án
- PRD rút gọn
- Cơ sở viết báo cáo phân tích thiết kế hệ thống
- Cơ sở dựng ERD, UML Class Diagram, API và backlog phát triển

