using Microsoft.AspNetCore.Mvc;
using VinhKhanh.Services;

namespace VinhKhanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PoiController : ControllerBase
    {
        private readonly GianHangService _gianHangService;

        public PoiController(GianHangService gianHangService)
        {
            _gianHangService = gianHangService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string lang = "vi")
        {
            var data = await _gianHangService.GetAllAsync(lang);

            var pois = data
                .Where(x => x.Lat.HasValue && x.Lon.HasValue)
                .Select(x => new
                {
                    id = x.IdGianHang,
                    ten = x.Ten,
                    lat = x.Lat,
                    lon = x.Lon
                })
                .ToList();

            return Ok(pois);
        }
    }
}