using Microsoft.AspNetCore.Mvc;
using VinhKhanh.Dtos;
using VinhKhanh.Services;

namespace VinhKhanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : ControllerBase
    {
        private readonly DeviceService _deviceService;

        public DeviceController(DeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpPost("activate")]
        public async Task<IActionResult> Activate([FromBody] ActivateDeviceRequestDto request)
        {
            var result = await _deviceService.ActivateAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("{maThietBi}/status")]
        public async Task<IActionResult> GetStatus(string maThietBi)
        {
            var result = await _deviceService.GetStatusAsync(maThietBi);

            if (!result.Found)
                return NotFound(result);

            return Ok(result);
        }
    }
}
