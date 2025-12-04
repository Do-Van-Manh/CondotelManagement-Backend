using CondotelManagement.DTOs.Host;
using CondotelManagement.Services.Interfaces.Host;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace CondotelManagement.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin/payouts")]
    public class AdminPayoutController : ControllerBase
    {
        private readonly IHostPayoutService _payoutService;

        public AdminPayoutController(IHostPayoutService payoutService)
        {
            _payoutService = payoutService;
        }

        /// <summary>
        /// Lấy danh sách booking chờ thanh toán cho host (Admin xem tất cả)
        /// </summary>
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingPayouts([FromQuery] int? hostId = null)
        {
            try
            {
                var pendingPayouts = await _payoutService.GetPendingPayoutsAsync(hostId);
                return Ok(new
                {
                    success = true,
                    data = pendingPayouts,
                    total = pendingPayouts.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting pending payouts", error = ex.Message });
            }
        }

        /// <summary>
        /// Xử lý tất cả các booking đủ điều kiện thanh toán cho host
        /// </summary>
        [HttpPost("process-all")]
        public async Task<IActionResult> ProcessAllPayouts()
        {
            try
            {
                var result = await _payoutService.ProcessPayoutsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing payouts", error = ex.Message });
            }
        }

        /// <summary>
        /// Xác nhận và xử lý payout cho một booking cụ thể
        /// </summary>
        [HttpPost("{bookingId}/confirm")]
        public async Task<IActionResult> ConfirmPayout(int bookingId)
        {
            try
            {
                var result = await _payoutService.ProcessPayoutForBookingAsync(bookingId);
                
                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error confirming payout", error = ex.Message });
            }
        }

        /// <summary>
        /// Lấy danh sách booking đã thanh toán cho host (đã hoàn thành và đã trả tiền)
        /// GET /api/admin/payouts/paid?hostId=1&fromDate=2025-01-01&toDate=2025-12-31
        /// </summary>
        [HttpGet("paid")]
        public async Task<IActionResult> GetPaidPayouts(
            [FromQuery] int? hostId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var paidPayouts = await _payoutService.GetPaidPayoutsAsync(hostId, fromDate, toDate);
                
                var totalAmount = paidPayouts.Sum(p => p.Amount);
                
                return Ok(new
                {
                    success = true,
                    data = paidPayouts,
                    total = paidPayouts.Count,
                    totalAmount = totalAmount,
                    summary = new
                    {
                        totalBookings = paidPayouts.Count,
                        totalRevenue = totalAmount,
                        averageAmount = paidPayouts.Count > 0 ? totalAmount / paidPayouts.Count : 0
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error getting paid payouts", error = ex.Message });
            }
        }
    }
}


