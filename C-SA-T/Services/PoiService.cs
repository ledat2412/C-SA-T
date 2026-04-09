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
                    IDChiNhanh = x.IdGianHang,
                    Title = x.Ten,
                    Subtitle = !string.IsNullOrWhiteSpace(x.MoTa) ? x.MoTa! : (x.DiaChi ?? string.Empty),
                    Latitude = x.Lat!.Value,
                    Longitude = x.Lon!.Value,
                    Address = x.DiaChi ?? string.Empty,
                    Description = x.MoTa ?? string.Empty,
                    ImagePath = x.HinhAnhChinh ?? x.HinhAnh
                })
                .ToList();
        }
    }
}
