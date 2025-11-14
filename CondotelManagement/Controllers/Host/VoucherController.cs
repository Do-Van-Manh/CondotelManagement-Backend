using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondotelManagement.Services.Interfaces;

namespace CondotelManagement.Controllers
{
	[ApiController]
	[Route("api/host/vouchers")]
	[Authorize(Roles = "Host")]
	public class VoucherController : ControllerBase
	{
		private readonly IVoucherService _voucherService;
		private readonly IHostService _hostService;

		public VoucherController(IVoucherService voucherService, IHostService hostService)
		{
			_voucherService = voucherService;
			_hostService = hostService;
		}

		[HttpGet]
		public async Task<IActionResult> GetVouchersByHost()
		{
			//current host login
			var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
			var vouchers = await _voucherService.GetVouchersByHostAsync(hostId);
			return Ok(vouchers);
		}

		[HttpPost]
		public async Task<IActionResult> CreateVoucher([FromBody] VoucherCreateDTO dto)
		{
			var created = await _voucherService.CreateVoucherAsync(dto);
			return CreatedAtAction(nameof(GetVouchersByHost), created);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateVoucher(int id, [FromBody] VoucherCreateDTO dto)
		{
			var updated = await _voucherService.UpdateVoucherAsync(id, dto);
			if (updated == null) return NotFound();
			return Ok(updated);
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteVoucher(int id)
		{
			var success = await _voucherService.DeleteVoucherAsync(id);
			if (!success) return NotFound();
			return NoContent();
		}
	}
}
