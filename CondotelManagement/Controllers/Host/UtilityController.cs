using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Services;
using CondotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Host
{
	[ApiController]
	[Route("api/host/utility")]
	[Authorize(Roles = "Host")]
	public class UtilityController : ControllerBase
	{
		private readonly IUtilitiesService _service;
		private readonly IHostService _hostService;

		public UtilityController(IUtilitiesService service, IHostService hostService)
		{
			_service = service;
			_hostService = hostService;
		}

		// ===========================
		// GET: /api/utility
		// Lấy tất cả Utilities của Host
		// ===========================
		[HttpGet]
		public async Task<IActionResult> GetByHost()
		{
			//current host login
			var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
			var result = await _service.GetUtilitiesByHostAsync(hostId);
			return Ok(result);
		}

		// ===========================
		// GET: /api/utility/5
		// Lấy Utility theo ID (kèm Host check)
		// ===========================
		[HttpGet("{utilityId}")]
		public async Task<IActionResult> GetById(int utilityId)
		{
			var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
			var result = await _service.GetByIdAsync(utilityId, hostId);

			if (result == null)
				return NotFound(new { message = "Tiện ích không tồn tại hoặc không thuộc về host này" });

			return Ok(result);
		}

		// ===========================
		// POST: /api/utility
		// tạo Utility (host chỉ tạo Utility của chính họ)
		// ===========================
		[HttpPost]
		public async Task<IActionResult> Create([FromBody] UtilityRequestDTO dto)
		{
			// Validate DataAnnotation
			if (!ModelState.IsValid)
			{
				return BadRequest(ApiResponse<object>.Fail(ModelState.ToErrorDictionary()));
			}

			var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
			var created = await _service.CreateAsync(hostId, dto);
			return Ok(ApiResponse<object>.SuccessResponse(created, "Đã tạo thành công"));
		}

		// ===========================
		// PUT: /api/utility/5/
		// update Utility (host = owner)
		// ===========================
		[HttpPut("{utilityId}")]
		public async Task<IActionResult> Update(int utilityId, [FromBody] UtilityRequestDTO dto)
		{
			// Validate DataAnnotation
			if (!ModelState.IsValid)
			{
				return BadRequest(ApiResponse<object>.Fail(ModelState.ToErrorDictionary()));
			}

			var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
			var success = await _service.UpdateAsync(utilityId, hostId, dto);

			if (!success)
				return NotFound(new { message = "Tiện ích không tồn tại hoặc không thuộc về host này" });

			return Ok(ApiResponse<object>.SuccessResponse(success, "Đã sửa thành công"));
		}

		// ===========================
		// DELETE: /api/utility/5
		// xóa Utility (host = owner)
		// ===========================
		[HttpDelete("{utilityId}")]
		public async Task<IActionResult> Delete(int utilityId)
		{
			var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
			var success = await _service.DeleteAsync(utilityId, hostId);

			if (!success)
				return NotFound(new { message = "Utility cannot be deleted because it is not found or does not belong to the host." });

			return Ok(new { message = "Delete successful" });
		}
	}
}
