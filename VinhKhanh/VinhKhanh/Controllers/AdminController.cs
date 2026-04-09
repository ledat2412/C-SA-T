using Microsoft.AspNetCore.Mvc;
using VinhKhanh.Dtos;
using VinhKhanh.Services;

namespace VinhKhanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AdminService _adminService;
        private readonly AccountAccessService _accountAccessService;

        public AdminController(AdminService adminService, AccountAccessService accountAccessService)
        {
            _adminService = adminService;
            _accountAccessService = accountAccessService;
        }

        private IActionResult ForbiddenResult()
        {
            return StatusCode(StatusCodes.Status403Forbidden, new OperationResultDto
            {
                Success = false,
                Message = "Tai khoan khong co quyen admin."
            });
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int idTaiKhoan)
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();

            return Ok(await _adminService.GetSummaryAsync());
        }

        [HttpGet("stores")]
        public async Task<IActionResult> GetStores([FromQuery] int idTaiKhoan)
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();

            return Ok(await _adminService.GetStoresAsync());
        }

        [HttpPost("stores")]
        public async Task<IActionResult> CreateStore([FromQuery] int idTaiKhoan, [FromBody] UpsertStoreRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();
            if (!request.IdChuQuanLy.HasValue)
                return BadRequest(new OperationResultDto { Success = false, Message = "Admin phai chi dinh idChuQuanLy." });

            return Ok(await storeManagementService.CreateStoreAsync(request, request.IdChuQuanLy.Value));
        }

        [HttpPut("stores/{idGianHang}")]
        public async Task<IActionResult> UpdateStore(int idGianHang, [FromQuery] int idTaiKhoan, [FromBody] UpsertStoreRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();

            var result = await storeManagementService.UpdateStoreAsync(idGianHang, request);
            if (result == null)
                return NotFound(new OperationResultDto { Success = false, Message = "Khong tim thay gian hang." });

            return Ok(result);
        }

        [HttpPatch("stores/{idGianHang}/status")]
        public async Task<IActionResult> UpdateStoreStatus(int idGianHang, [FromQuery] int idTaiKhoan, [FromBody] UpdateStoreStatusRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();

            var result = await storeManagementService.UpdateStoreStatusAsync(idGianHang, request.TinhTrang);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("stores/{idGianHang}/foods")]
        public async Task<IActionResult> GetStoreFoods(int idGianHang, [FromQuery] int idTaiKhoan, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();

            return Ok(await storeManagementService.GetFoodsByStoreAsync(idGianHang));
        }

        [HttpPost("foods")]
        public async Task<IActionResult> CreateFood([FromQuery] int idTaiKhoan, [FromBody] UpsertFoodRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();

            return Ok(await storeManagementService.CreateFoodAsync(request));
        }

        [HttpPut("foods/{idMonAn}")]
        public async Task<IActionResult> UpdateFood(int idMonAn, [FromQuery] int idTaiKhoan, [FromBody] UpsertFoodRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();

            var result = await storeManagementService.UpdateFoodAsync(idMonAn, request);
            if (result == null)
                return NotFound(new OperationResultDto { Success = false, Message = "Khong tim thay mon an." });

            return Ok(result);
        }

        [HttpPatch("foods/{idMonAn}/status")]
        public async Task<IActionResult> UpdateFoodStatus(int idMonAn, [FromQuery] int idTaiKhoan, [FromBody] UpdateFoodStatusRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();

            var result = await storeManagementService.UpdateFoodStatusAsync(idMonAn, request.TinhTrang);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}
