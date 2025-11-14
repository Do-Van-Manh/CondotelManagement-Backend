using CondotelManagement.DTOs.Package;
using CondotelManagement.Services.Interfaces;
using CondotelManagement.Services.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks; // Them Using
using System; // Them Using
using System.Linq; // Them Using

// Them alias de tranh loi 'Host'
using HostModel = CondotelManagement.Models.Host;

namespace CondotelManagement.Controllers.Host
{
    [Route("api/host/packages")]
    [ApiController]
    [Authorize(Roles = "Host")] // Chi Host
    public class HostPackageController : ControllerBase // SUA: Phai la ControllerBase
    {
        private readonly IPackageService _packageService;
        private readonly IAuthService _authService;

        public HostPackageController(IPackageService packageService, IAuthService authService)
        {
            _packageService = packageService;
            _authService = authService;
        }

        // Lấy HostId từ token
        private async Task<int> GetCurrentHostId()
        {
            var user = await _authService.GetCurrentUserAsync(); // (Ham nay da Include Role)
            if (user == null)
            {
                throw new Exception("Xac thuc that bai.");
            }

            // Lay Host tu User (Model User co ICollection<Host> Hosts)
            var host = user.Hosts.FirstOrDefault();
            if (host == null)
            {
                // Logic nay can ban xem lai:
                // Neu user co Role "Host" nhung chua co record [Host] tuong ung
                // thi se bi loi. 
                throw new Exception("Khong tim thay thong tin Host tuong ung voi UserID nay.");
            }
            return host.HostId;
        }

        [HttpGet("my-package")]
        public async Task<IActionResult> GetMyPackage()
        {
            try
            {
                var hostId = await GetCurrentHostId();
                var myPackage = await _packageService.GetMyActivePackageAsync(hostId);
                if (myPackage == null)
                {
                    // Tra ve 200 OK voi message thay vi 404
                    return Ok(new { message = "Bạn hiện không có gói dịch vụ nào." });
                }
                return Ok(myPackage);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("purchase")]
        public async Task<IActionResult> PurchasePackage([FromBody] PurchasePackageRequestDto request)
        {
            if (request.PackageId <= 0)
            {
                return BadRequest(new { message = "PackageId không hợp lệ." });
            }

            try
            {
                var hostId = await GetCurrentHostId();
                var newPackage = await _packageService.PurchaseOrUpgradePackageAsync(hostId, request.PackageId);
                return Ok(newPackage);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    } // Ket thuc Class
} // Ket thuc Namespace