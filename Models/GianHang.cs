namespace MauiApp1.Models;

public class GianHang
{
    public int IdGianHang { get; set; }
    public string Ten { get; set; } = "";

    public string? HinhAnh { get; set; }

    public string? DiaChi { get; set; }
    public string? MoTa { get; set; }
    public double? Lat { get; set; }
    public double? Lon { get; set; }
    public string? TinhTrang { get; set; }
    public decimal PhiHangThang { get; set; }
    public DateTime NgayDangKy { get; set; }
    public DateTime? ThoiGianCapNhat { get; set; }
}