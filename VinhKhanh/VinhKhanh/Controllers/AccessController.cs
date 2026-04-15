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
        public async Task<IActionResult> Validate([FromQuery] string accessToken, [FromQuery] string? clientDeviceId = null)
        {
            var result = await _accessSessionService.ValidateAsync(accessToken, clientDeviceId);
            return Ok(result);
        }

        [HttpPost("recover")]
        public async Task<IActionResult> Recover([FromBody] RecoverAccessRequestDto request)
        {
            var result = await _accessSessionService.RecoverAccessAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("package/register")]
        public async Task<IActionResult> RegisterPackage([FromBody] RegisterPackageAccessRequestDto request)
        {
            var result = await _accessSessionService.RegisterPackageAccessAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}
