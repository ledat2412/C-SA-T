<<<<<<< HEAD
using MauiApp1.Models;
=======
﻿using MauiApp1.Models;
>>>>>>> 2e8f476d66d60fa26b4337d437e259a134102afb

namespace MauiApp1.Services
{
    public class PoiService
    {
        private readonly GianHangService _gianHangService;
        private readonly HinhAnhChiNhanhService _hinhAnhChiNhanhService;

        public PoiService(
            GianHangService gianHangService,
            HinhAnhChiNhanhService hinhAnhChiNhanhService)
        {
            _gianHangService = gianHangService;
            _hinhAnhChiNhanhService = hinhAnhChiNhanhService;
        }

        public async Task<List<PoiItem>> GetAllPoisAsync()
        {
            var result = new List<PoiItem>();
            var gianHangs = await _gianHangService.GetAllAsync();

            foreach (var gh in gianHangs)
            {
                if (gh.Lat == null || gh.Lon == null)
                    continue;

                var imagePath = await _hinhAnhChiNhanhService
                    .GetFirstImagePathByChiNhanhAsync(gh.IdGianHang);

                result.Add(new PoiItem
                {
                    IDChiNhanh = gh.IdGianHang,
                    Title = string.IsNullOrWhiteSpace(gh.Ten)
                        ? $"Gian hàng #{gh.IdGianHang}"
                        : gh.Ten,
                    Subtitle = !string.IsNullOrWhiteSpace(gh.MoTa)
                        ? gh.MoTa
                        : gh.DiaChi,
                    ImagePath = NormalizeImagePath(imagePath),
                    Latitude = gh.Lat.Value,
                    Longitude = gh.Lon.Value
                });
            }

            return result;
        }

        private string NormalizeImagePath(string? dbPath)
        {
            if (string.IsNullOrWhiteSpace(dbPath))
<<<<<<< HEAD
                return "dotnet_bot.png";

            if (!dbPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !dbPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return dbPath.Trim();
            }

            return dbPath.Trim();
        }
    }
}
=======
                return "chucchich.jpg";

            // Nếu DB lưu tên file có sẵn trong Resources/Images
            // ví dụ: chucchich.jpg, mypham.jpg, tet.jpg
            if (!dbPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !dbPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return dbPath;
            }

            // Nếu DB lưu URL đầy đủ
            return dbPath;
        }
    }
}
>>>>>>> 2e8f476d66d60fa26b4337d437e259a134102afb
