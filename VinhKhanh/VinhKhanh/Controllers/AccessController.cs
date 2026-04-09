using Microsoft.AspNetCore.Mvc;
using VinhKhanh.Dtos;
using VinhKhanh.Services;

namespace VinhKhanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccessController : ControllerBase
    {
        private readonly AccessSessionService _accessSessionService;

        public AccessController(AccessSessionService accessSessionService)
        {
            _accessSessionService = accessSessionService;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> Scan([FromBody] ScanQrRequestDto request)
        {
            var result = await _accessSessionService.CreateFromQrAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("validate")]
        public async Task<IActionResult> Validate([FromQuery] string accessToken)
        {
            var result = await _accessSessionService.ValidateAsync(accessToken);
            return Ok(result);
        }
    }
}
