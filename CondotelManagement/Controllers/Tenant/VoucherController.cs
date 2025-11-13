using CondotelManagement.DTOs;
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

		[HttpPost("create-after-booking")]
		public async Task<IActionResult> CreateVoucherAfterBooking([FromBody] VoucherAutoCreateDTO dto)
		{
			var voucher = await _voucherService.CreateVoucherAfterBookingAsync(dto);
			if (voucher == null) return BadRequest("Cannot create voucher");

			return Ok(voucher);
		}


		//[HttpGet("condotel/{condotelId}")]
		//public async Task<IActionResult> GetVouchersByCondotel(int condotelId)
		//{
		//	var vouchers = await _voucherService.GetVouchersByCondotelAsync(condotelId);
		//	return Ok(vouchers);
		//}
	}
}
