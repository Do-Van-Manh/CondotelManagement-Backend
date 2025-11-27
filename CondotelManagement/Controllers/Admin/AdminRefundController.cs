using CondotelManagement.DTOs.Admin;
using CondotelManagement.Services.Interfaces.BookingService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/refunds")]
    public class AdminRefundController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public AdminRefundController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Lấy danh sách yêu cầu hoàn tiền với filter
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetRefundRequests(
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? status = "all",
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var refundRequests = await _bookingService.GetRefundRequestsAsync(searchTerm, status, startDate, endDate);

            return Ok(new
            {
                success = true,
                data = refundRequests,
                total = refundRequests.Count
            });
        }

        /// <summary>
        /// Xác nhận đã chuyển tiền thủ công (không dùng PayOS API tự động)
        /// </summary>
        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmRefund(int id)
        {
            var result = await _bookingService.ConfirmRefundManually(id);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}





