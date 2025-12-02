using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Services;
using CondotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Host
{
	[ApiController]
	[Route("api/host/settings/voucher")]
	//[Authorize(Roles = "Host")]
	public class HostVoucherSettingController : ControllerBase
	{
		private readonly IVoucherService _voucherService;
		private readonly IHostService _hostService;
		public HostVoucherSettingController(IVoucherService voucherService, IHostService hostService)
		{
			_voucherService = voucherService;
			_hostService = hostService;
		}
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			//current host login
			var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
			var result = await _voucherService.GetSettingAsync(hostId);
			return Ok(result);
		}

		[HttpPost]
		public async Task<IActionResult> Save(HostVoucherSettingDTO dto)
		{
			//current host login
			var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
			var result = await _voucherService.SaveSettingAsync(hostId, dto);
			return Ok(result);
		}
	}
}
