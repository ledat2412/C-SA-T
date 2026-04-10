using MauiApp1.Models;
using System.Globalization;

namespace MauiApp1.Services
{
    public class PoiService
    {
        private readonly AppDataCacheService _cacheService;

        public PoiService(AppDataCacheService cacheService)
        {
            _cacheService = cacheService;
        }

        private static string GetCurrentLang()
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName?.ToLowerInvariant();
            return string.IsNullOrWhiteSpace(lang) ? "vi" : lang;
        }

        public async Task<List<PoiItem>> GetAllPoisAsync()
        {
            var lang = GetCurrentLang();
            var appData = await _cacheService.GetAsync(lang);

            return appData.GianHangs
                .Where(x => x.Lat.HasValue && x.Lon.HasValue)
                .Select(x => new PoiItem
                {
                    MenuNames = x.MonAns
                        .Where(m => !string.Equals(m.TinhTrang, "an", StringComparison.OrdinalIgnoreCase))
                        .Select(m => m.Ten?.Trim())
                        .Where(name => !string.IsNullOrWhiteSpace(name))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray()!,
                    IDChiNhanh = x.IdGianHang,
                    Title = x.Ten,
                    Subtitle = !string.IsNullOrWhiteSpace(x.MoTa) ? x.MoTa! : (x.DiaChi ?? string.Empty),
                    Latitude = x.Lat!.Value,
                    Longitude = x.Lon!.Value,
                    Address = x.DiaChi ?? string.Empty,
                    Description = x.MoTa ?? string.Empty,
                    ImagePath = x.HinhAnhChinh ?? x.HinhAnh,
                    SearchText = BuildSearchText(x)
                })
                .ToList();
        }

        private static string BuildSearchText(GianHang gianHang)
        {
            var parts = new List<string>
            {
                gianHang.Ten,
                gianHang.DiaChi ?? string.Empty,
                gianHang.MoTa ?? string.Empty
            };

            parts.AddRange(gianHang.MonAns
                .Where(m => !string.Equals(m.TinhTrang, "an", StringComparison.OrdinalIgnoreCase))
                .Select(m => $"{m.Ten} {m.MoTa}".Trim()));

            return string.Join(" ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
