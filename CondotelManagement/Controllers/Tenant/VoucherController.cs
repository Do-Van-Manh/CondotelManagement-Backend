using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Models;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Tenant
{
    [ApiController]
    [Route("api/vouchers")]
    public class VoucherController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VoucherController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        [HttpGet("condotel/{condotelId}")]
        public async Task<IActionResult> GetVouchersByCondotel(int condotelId)
        {
            var vouchers = await _voucherService.GetVouchersByCondotelAsync(condotelId);
            return Ok(ApiResponse<object>.SuccessResponse(vouchers));
        }

		[HttpPost("auto-create/{bookingId}")]
		public async Task<IActionResult> CreateVoucherAfterBooking(int bookingId)
		{
			if (bookingId <= 0)
				return BadRequest(ApiResponse<object>.Fail("BookingID không hợp lệ."));

			var vouchers = await _voucherService.CreateVoucherAfterBookingAsync(bookingId);

			if (vouchers == null || vouchers.Count == 0)
				return Ok(ApiResponse<object>.Fail("Không thể tạo voucher – có thể host tắt AutoGenerate hoặc setting chưa cấu hình."));
			return Ok(ApiResponse<object>.SuccessResponse(vouchers, $"Đã tạo {vouchers.Count} voucher cho user."));
		}
	}
}
