namespace MauiApp1.Models;

public class PoiItem
{
    public int IDChiNhanh { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public string ImagePath { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }
}