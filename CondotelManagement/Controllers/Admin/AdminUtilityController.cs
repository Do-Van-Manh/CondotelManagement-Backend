using CondotelManagement.DTOs;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Admin
{
	[ApiController]
	[Route("api/admin/utilities")]
	[Authorize(Roles = "Admin")]
	public class AdminUtilityController : ControllerBase
	{
		private readonly IUtilitiesService _utilitiesService;

		public AdminUtilityController(IUtilitiesService utilitiesService)
		{
			_utilitiesService = utilitiesService;
		}

		/// <summary>
		/// Lấy tất cả utilities
		/// GET /api/admin/utilities
		/// </summary>
		[HttpGet]
		public async Task<ActionResult<IEnumerable<UtilityResponseDTO>>> GetAll()
		{
			var utilities = await _utilitiesService.AdminGetAllAsync();
			return Ok(new
			{
				success = true,
				data = utilities,
				total = utilities.Count()
			});
		}

		/// <summary>
		/// Lấy utility theo ID
		/// GET /api/admin/utilities/{id}
		/// </summary>
		[HttpGet("{id}")]
		public async Task<ActionResult<UtilityResponseDTO>> GetById(int id)
		{
			var utility = await _utilitiesService.AdminGetByIdAsync(id);
			if (utility == null)
				return NotFound(new { success = false, message = "Utility not found" });

			return Ok(new { success = true, data = utility });
		}

		/// <summary>
		/// Tạo utility mới
		/// POST /api/admin/utilities
		/// </summary>
		[HttpPost]
		public async Task<ActionResult<UtilityResponseDTO>> Create([FromBody] UtilityRequestDTO dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

			var created = await _utilitiesService.AdminCreateAsync(dto);
			return CreatedAtAction(nameof(GetById), new { id = created.UtilityId }, new
			{
				success = true,
				message = "Utility created successfully",
				data = created
			});
		}

		/// <summary>
		/// Cập nhật utility
		/// PUT /api/admin/utilities/{id}
		/// </summary>
		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] UtilityRequestDTO dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

			var updated = await _utilitiesService.AdminUpdateAsync(id, dto);
			if (!updated)
				return NotFound(new { success = false, message = "Utility not found" });

			return Ok(new { success = true, message = "Utility updated successfully" });
		}

		/// <summary>
		/// Xóa utility
		/// DELETE /api/admin/utilities/{id}
		/// </summary>
		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			var deleted = await _utilitiesService.AdminDeleteAsync(id);
			if (!deleted)
				return NotFound(new { success = false, message = "Utility not found" });

			return Ok(new { success = true, message = "Utility deleted successfully" });
		}
	}
}

