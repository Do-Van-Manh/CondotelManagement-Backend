using CondotelManagement.DTOs.Wallet;
using CondotelManagement.Services.Interfaces.Wallet;
using CondotelManagement.Services.Interfaces.Host;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using CondotelManagement.Services.Interfaces;

namespace CondotelManagement.Controllers.Host
{
    [ApiController]
    [Route("api/host/wallet")]
    [Authorize(Roles = "Host")]
    public class HostWalletController : ControllerBase
    {
        private readonly IWalletService _walletService;
        private readonly IHostService _hostService;

        public HostWalletController(IWalletService walletService, IHostService hostService)
        {
            _walletService = walletService;
            _hostService = hostService;
        }

        /// <summary>
        /// Lấy danh sách tài khoản ngân hàng của host
        /// GET /api/host/wallet
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyWallets()
        {
            var hostId = GetHostId();
            var wallets = await _walletService.GetWalletsByHostIdAsync(hostId);
            
            return Ok(new
            {
                success = true,
                data = wallets,
                total = wallets.Count()
            });
        }

        /// <summary>
        /// Tạo tài khoản ngân hàng mới cho host
        /// POST /api/host/wallet
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateWallet([FromBody] WalletCreateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

            var hostId = GetHostId();
            dto.HostId = hostId; // Set hostId từ token

            var created = await _walletService.CreateWalletAsync(dto);
            return CreatedAtAction(nameof(GetMyWallets), null, new
            {
                success = true,
                message = "Wallet created successfully",
                data = created
            });
        }

        /// <summary>
        /// Cập nhật tài khoản ngân hàng
        /// PUT /api/host/wallet/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWallet(int id, [FromBody] WalletUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

            var wallet = await _walletService.GetWalletByIdAsync(id);
            if (wallet == null)
                return NotFound(new { success = false, message = "Wallet not found" });

            var hostId = GetHostId();
            if (wallet.HostId != hostId)
                return Forbid();

            var updated = await _walletService.UpdateWalletAsync(id, dto);
            if (!updated)
                return BadRequest(new { success = false, message = "Failed to update wallet" });

            return Ok(new { success = true, message = "Wallet updated successfully" });
        }

        /// <summary>
        /// Xóa tài khoản ngân hàng
        /// DELETE /api/host/wallet/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWallet(int id)
        {
            var wallet = await _walletService.GetWalletByIdAsync(id);
            if (wallet == null)
                return NotFound(new { success = false, message = "Wallet not found" });

            var hostId = GetHostId();
            if (wallet.HostId != hostId)
                return Forbid();

            var deleted = await _walletService.DeleteWalletAsync(id);
            if (!deleted)
                return BadRequest(new { success = false, message = "Failed to delete wallet" });

            return Ok(new { success = true, message = "Wallet deleted successfully" });
        }

        /// <summary>
        /// Đặt tài khoản ngân hàng làm mặc định
        /// POST /api/host/wallet/{id}/set-default
        /// </summary>
        [HttpPost("{id}/set-default")]
        public async Task<IActionResult> SetDefaultWallet(int id)
        {
            var wallet = await _walletService.GetWalletByIdAsync(id);
            if (wallet == null)
                return NotFound(new { success = false, message = "Wallet not found" });

            var hostId = GetHostId();
            if (wallet.HostId != hostId)
                return Forbid();

            var result = await _walletService.SetDefaultWalletAsync(id, null, hostId);
            if (!result)
                return BadRequest(new { success = false, message = "Failed to set default wallet" });

            return Ok(new { success = true, message = "Default wallet set successfully" });
        }

        private int GetHostId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            var host = _hostService.GetByUserId(userId);
            if (host == null)
                throw new UnauthorizedAccessException("Host not found");

            return host.HostId;
        }
    }
}

