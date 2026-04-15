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

        [HttpGet("stores/{idGianHang}")]
        public async Task<IActionResult> GetStore(int idGianHang, [FromQuery] int idTaiKhoan, [FromServices] StoreManagementService storeManagementService, [FromQuery] string lang = "vi")
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();

            var result = await storeManagementService.GetStoreByIdAsync(idGianHang, lang);
            if (result == null)
                return NotFound(new OperationResultDto { Success = false, Message = "Khong tim thay gian hang." });

            return Ok(result);
        }

        [HttpGet("owners")]
        public async Task<IActionResult> GetOwners([FromQuery] int idTaiKhoan)
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();

            return Ok(await _adminService.GetOwnersAsync());
        }

        [HttpPost("stores")]
        public async Task<IActionResult> CreateStore([FromQuery] int idTaiKhoan, [FromBody] UpsertStoreRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();

            var ownerId = await ResolveOwnerIdAsync(request);
            if (!ownerId.HasValue)
                return BadRequest(new OperationResultDto { Success = false, Message = "Admin phai chi dinh chu quan ly hop le." });

            request.IdChuQuanLy = ownerId.Value;
            return Ok(await storeManagementService.CreateStoreAsync(request, ownerId.Value));
        }

        [HttpPut("stores/{idGianHang}")]
        public async Task<IActionResult> UpdateStore(int idGianHang, [FromQuery] int idTaiKhoan, [FromBody] UpsertStoreRequestDto request, [FromServices] StoreManagementService storeManagementService)
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();

            var ownerId = await ResolveOwnerIdAsync(request);
            if (ownerId.HasValue)
                request.IdChuQuanLy = ownerId.Value;

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

        [HttpPost("stores/{idGianHang}/image")]
        public async Task<IActionResult> UploadStoreImage(int idGianHang, [FromQuery] int idTaiKhoan, [FromForm] IFormFile image, [FromServices] StoreManagementService storeManagementService, [FromServices] IWebHostEnvironment env)
        {
            if (!await _accountAccessService.IsAdminAsync(idTaiKhoan))
                return ForbiddenResult();
            if (image == null || image.Length <= 0)
                return BadRequest(new OperationResultDto { Success = false, Message = "Vui long chon anh hop le." });

            var imagePath = await storeManagementService.SaveStoreImageAsync(idGianHang, image, env);
            if (imagePath == null)
                return NotFound(new OperationResultDto { Success = false, Message = "Khong tim thay gian hang." });

            return Ok(new
            {
                success = true,
                message = "Cap nhat anh gian hang thanh cong.",
                imagePath
            });
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

        private async Task<int?> ResolveOwnerIdAsync(UpsertStoreRequestDto request)
        {
            if (request.IdChuQuanLy.HasValue && request.IdChuQuanLy.Value > 0)
                return request.IdChuQuanLy.Value;

            if (!string.IsNullOrWhiteSpace(request.EmailChuQuanLy))
                return await _adminService.GetOwnerIdByEmailAsync(request.EmailChuQuanLy);

            return null;
        }
    }
}
