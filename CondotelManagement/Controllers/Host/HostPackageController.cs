using CondotelManagement.Data;
using CondotelManagement.DTOs.Package;
using CondotelManagement.Models;                    // ← Thêm để dùng Host
using CondotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;                 // ← Quan trọng: để dùng FirstOrDefaultAsync
using System.Security.Claims;
using System.Threading.Tasks;

namespace CondotelManagement.Controllers.Host
{
    [Route("api/host/packages")]
    [ApiController]
    [Authorize(Roles = "Host")]
    public class HostPackageController : ControllerBase
    {
        private readonly IPackageService _packageService;
        private readonly CondotelDbVer1Context _context;   // ← Thêm DbContext (hoặc IHostRepository nếu có)

        // Nếu bạn có IHostRepository thì inject nó thay cho _context cũng được
        public HostPackageController(IPackageService packageService, CondotelDbVer1Context context)
        {
            _packageService = packageService;
            _context = context;
        }

        /// <summary>
        /// Lấy HostId từ bảng Hosts một cách an toàn, không phụ thuộc vào GetCurrentUserAsync()
        /// </summary>
        private async Task<int?> GetCurrentHostIdOrNullAsync()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
                return null;

            var host = await _context.Hosts
                .FirstOrDefaultAsync(h => h.UserId == userId);

            return host?.HostId;
        }

        /// <summary>
        /// GET api/host/packages/my-package
        /// </summary>
        [HttpGet("my-package")]
        public async Task<IActionResult> GetMyPackage()
        {
            var hostId = await GetCurrentHostIdOrNullAsync();

            // Chưa đăng ký làm Host → trả null (FE sẽ hiện form hoặc thông báo)
            if (!hostId.HasValue)
                return Ok(null);

            var package = await _packageService.GetMyActivePackageAsync(hostId.Value);
            return Ok(package ?? null);
        }

        /// <summary>
        /// POST api/host/packages/purchase
        /// </summary>
        [HttpPost("purchase")]
        public async Task<IActionResult> PurchasePackage([FromBody] PurchasePackageRequestDto request)
        {
            if (request == null || request.PackageId <= 0)
                return BadRequest(new { message = "PackageId không hợp lệ." });

            var hostId = await GetCurrentHostIdOrNullAsync();

            // Chưa hoàn tất đăng ký Host → chặn mua gói
            if (!hostId.HasValue)
                return BadRequest(new { message = "Vui lòng hoàn tất đăng ký làm Host trước khi mua gói dịch vụ." });

            try
            {
                var result = await _packageService.PurchaseOrUpgradePackageAsync(hostId.Value, request.PackageId);
                return Ok(result);   // result thường là PaymentUrl hoặc thông tin gói mới
            }
            catch (Exception ex)
            {
                // Log ex nếu cần
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}