using MauiApp1.Models;
using System.Globalization;

namespace MauiApp1.Services
{
    public class MonAnService
    {
        private readonly AppDataCacheService _cacheService;

        public MonAnService(AppDataCacheService cacheService)
        {
            _cacheService = cacheService;
        }

        private static string GetCurrentLang()
        {
            var lang = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName?.ToLowerInvariant();
            return string.IsNullOrWhiteSpace(lang) ? "vi" : lang;
        }

        public async Task<List<MonAn>> GetByGianHangAsync(int idGianHang, string? lang = null)
        {
            var currentLang = lang ?? GetCurrentLang();
            var appData = await _cacheService.GetAsync(currentLang);

            return appData.GianHangs
                .FirstOrDefault(x => x.IdGianHang == idGianHang)?
                .MonAns ?? new List<MonAn>();
        }

        public async Task<List<MonAn>> GetByChiNhanhAsync(int idChiNhanh, string? lang = null)
        {
            return await GetByGianHangAsync(idChiNhanh, lang);
        }

        public async Task<List<MonAn>> GetAllAsync(string? lang = null)
        {
            var currentLang = lang ?? GetCurrentLang();
            var appData = await _cacheService.GetAsync(currentLang);

            return appData.GianHangs
                .SelectMany(x => x.MonAns)
                .ToList();
        }
    }
}