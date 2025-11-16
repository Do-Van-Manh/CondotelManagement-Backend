using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Host
{
	[ApiController]
	[Route("api/host")]
	[Authorize(Roles = "Host")]
	public class ProfileController : ControllerBase
	{
		private readonly IHostService _hostService;

		public ProfileController(IHostService hostService)
		{
			_hostService = hostService;
		}

		[HttpGet("profile")]
		public async Task<IActionResult> GetProfile()
		{
			var result = await _hostService.GetHostProfileAsync(User.GetUserId());

			if (result == null)
				return NotFound(new { message = "Host not found" });

			return Ok(result);
		}

		[HttpPut("profile")]
		public async Task<IActionResult> UpdateProfile([FromBody] UpdateHostProfileDTO dto)
		{
			var result = await _hostService.UpdateHostProfileAsync(User.GetUserId(), dto);
			if (!result)
				return BadRequest(new { message = "Update failed" });

			return Ok(new { message = "Profile updated successfully" });
		}
	}
}
