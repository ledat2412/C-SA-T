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

        [HttpPost("{id}/visit")]
        public async Task<IActionResult> RecordVisit(int id)
        {
            var deviceId = Request.Headers["X-Device-Id"].FirstOrDefault();
            var result = await _gianHangService.IncrementVisitCountAsync(id, deviceId);

            if (!result.StoreExists)
            {
                return NotFound(new { success = false, message = "Khong tim thay gian hang." });
            }

            if (!result.Success)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    success = false,
                    message = "Khong the ghi nhan luot truy cap POI."
                });
            }

            return Ok(new
            {
                success = true,
                counted = result.Counted,
                message = result.Counted
                    ? "Da ghi nhan luot truy cap POI."
                    : "Thiet bi nay da duoc ghi nhan cho gian hang nay trong ngay hom nay."
            });
        }
    }
}
