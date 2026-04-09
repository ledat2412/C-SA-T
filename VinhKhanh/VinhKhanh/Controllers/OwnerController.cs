using Microsoft.AspNetCore.Mvc;
using VinhKhanh.Dtos;
using VinhKhanh.Services;

namespace VinhKhanh.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnerController : ControllerBase
    {
        private readonly OwnerService _ownerService;
        private readonly AccountAccessService _accountAccessService;

        public OwnerController(OwnerService ownerService, AccountAccessService accountAccessService)
        {
            _ownerService = ownerService;
            _accountAccessService = accountAccessService;
        }

        private IActionResult ForbiddenResult()
        {
            return StatusCode(StatusCodes.Status403Forbidden, new OperationResultDto
            {
                Success = false,
                Message = "Tai khoan khong co quyen chu cua hang."
            });
        }

        [HttpGet("stores")]
        public async Task<IActionResult> GetMyStores([FromQuery] int idTaiKhoan)
        {
            if (!await _accountAccessService.IsOwnerAsync(idTaiKhoan))
                return ForbiddenResult();

            return Ok(await _ownerService.GetStoresByAccountAsync(idTaiKhoan));
        }

        [HttpPost("stores")]
        public async Task<IActionResult> CreateStore([FromQuery] int idTaiKhoan, [FromBody] UpsertStoreRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsOwnerAsync(idTaiKhoan))
                return ForbiddenResult();

            var result = await _ownerService.CreateStoreAsync(idTaiKhoan, request, storeManagementService);
            return Ok(result);
        }

        [HttpPut("stores/{idGianHang}")]
        public async Task<IActionResult> UpdateStore(int idGianHang, [FromQuery] int idTaiKhoan, [FromBody] UpsertStoreRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsStoreOwnedByAccountAsync(idTaiKhoan, idGianHang))
                return ForbiddenResult();

            var result = await storeManagementService.UpdateStoreAsync(idGianHang, request);
            if (result == null)
                return NotFound(new OperationResultDto { Success = false, Message = "Khong tim thay gian hang." });

            return Ok(result);
        }

        [HttpPatch("stores/{idGianHang}/status")]
        public async Task<IActionResult> UpdateStoreStatus(int idGianHang, [FromQuery] int idTaiKhoan, [FromBody] UpdateStoreStatusRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsStoreOwnedByAccountAsync(idTaiKhoan, idGianHang))
                return ForbiddenResult();

            var result = await storeManagementService.UpdateStoreStatusAsync(idGianHang, request.TinhTrang);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }

        [HttpGet("stores/{idGianHang}/foods")]
        public async Task<IActionResult> GetStoreFoods(int idGianHang, [FromQuery] int idTaiKhoan, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsStoreOwnedByAccountAsync(idTaiKhoan, idGianHang))
                return ForbiddenResult();

            return Ok(await storeManagementService.GetFoodsByStoreAsync(idGianHang));
        }

        [HttpPost("foods")]
        public async Task<IActionResult> CreateFood([FromQuery] int idTaiKhoan, [FromBody] UpsertFoodRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsStoreOwnedByAccountAsync(idTaiKhoan, request.IdGianHang))
                return ForbiddenResult();

            return Ok(await storeManagementService.CreateFoodAsync(request));
        }

        [HttpPut("foods/{idMonAn}")]
        public async Task<IActionResult> UpdateFood(int idMonAn, [FromQuery] int idTaiKhoan, [FromBody] UpsertFoodRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsFoodOwnedByAccountAsync(idTaiKhoan, idMonAn))
                return ForbiddenResult();
            if (!await _accountAccessService.IsStoreOwnedByAccountAsync(idTaiKhoan, request.IdGianHang))
                return ForbiddenResult();

            var result = await storeManagementService.UpdateFoodAsync(idMonAn, request);
            if (result == null)
                return NotFound(new OperationResultDto { Success = false, Message = "Khong tim thay mon an." });

            return Ok(result);
        }

        [HttpPatch("foods/{idMonAn}/status")]
        public async Task<IActionResult> UpdateFoodStatus(int idMonAn, [FromQuery] int idTaiKhoan, [FromBody] UpdateFoodStatusRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsFoodOwnedByAccountAsync(idTaiKhoan, idMonAn))
                return ForbiddenResult();

            var result = await storeManagementService.UpdateFoodStatusAsync(idMonAn, request.TinhTrang);
            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
    }
}
