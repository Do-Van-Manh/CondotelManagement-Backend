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
			return Ok(vouchers);
		}
	}
}
