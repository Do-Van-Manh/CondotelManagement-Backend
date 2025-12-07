using CondotelManagement.DTOs;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/locations")]
    [Authorize(Roles = "Admin")]
    public class AdminLocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public AdminLocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        /// <summary>
        /// Lấy tất cả locations
        /// GET /api/admin/locations
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationDTO>>> GetAll()
        {
            var locations = await _locationService.GetAllAsync();
            return Ok(new
            {
                success = true,
                data = locations,
                total = locations.Count()
            });
        }

        /// <summary>
        /// Lấy location theo ID
        /// GET /api/admin/locations/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDTO>> GetById(int id)
        {
            var location = await _locationService.GetByIdAsync(id);
            if (location == null)
                return NotFound(new { success = false, message = "Không tìm thấy vị trí" });
            
            return Ok(new { success = true, data = location });
        }

        /// <summary>
        /// Tạo location mới
        /// POST /api/admin/locations
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<LocationDTO>> Create([FromBody] LocationCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });

            var created = await _locationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.LocationId }, new
            {
                success = true,
                message = "Vị trí đã được tạo thành công",
                data = created
            });
        }

        /// <summary>
        /// Cập nhật location
        /// PUT /api/admin/locations/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] LocationCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ", errors = ModelState });

            var updated = await _locationService.UpdateAsync(id, dto);
            if (!updated)
                return NotFound(new { success = false, message = "Không tìm thấy vị trí" });
            
            return Ok(new { success = true, message = "Vị trí đã được cập nhật thành công" });
        }

        /// <summary>
        /// Xóa location
        /// DELETE /api/admin/locations/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _locationService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { success = false, message = "Không tìm thấy vị trí" });
            
            return Ok(new { success = true, message = "Đã xóa vị trí thành công" });
        }
    }
}

