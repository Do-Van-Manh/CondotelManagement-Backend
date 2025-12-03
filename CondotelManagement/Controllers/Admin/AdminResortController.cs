using CondotelManagement.DTOs;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/resorts")]
    [Authorize(Roles = "Admin")]
    public class AdminResortController : ControllerBase
    {
        private readonly IResortService _resortService;

        public AdminResortController(IResortService resortService)
        {
            _resortService = resortService;
        }

        /// <summary>
        /// Lấy tất cả resorts
        /// GET /api/admin/resorts
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ResortDTO>>> GetAll()
        {
            var resorts = await _resortService.GetAllAsync();
            return Ok(new
            {
                success = true,
                data = resorts,
                total = resorts.Count()
            });
        }

        /// <summary>
        /// Lấy resort theo ID
        /// GET /api/admin/resorts/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResortDTO>> GetById(int id)
        {
            var resort = await _resortService.GetByIdAsync(id);
            if (resort == null)
                return NotFound(new { success = false, message = "Resort not found" });
            
            return Ok(new { success = true, data = resort });
        }

        /// <summary>
        /// Lấy resorts theo LocationId
        /// GET /api/admin/resorts/location/{locationId}
        /// </summary>
        [HttpGet("location/{locationId}")]
        public async Task<ActionResult<IEnumerable<ResortDTO>>> GetByLocationId(int locationId)
        {
            var resorts = await _resortService.GetByLocationIdAsync(locationId);
            return Ok(new
            {
                success = true,
                data = resorts,
                total = resorts.Count()
            });
        }

        /// <summary>
        /// Tạo resort mới
        /// POST /api/admin/resorts
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ResortDTO>> Create([FromBody] ResortCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

            var created = await _resortService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.ResortId }, new
            {
                success = true,
                message = "Resort created successfully",
                data = created
            });
        }

        /// <summary>
        /// Cập nhật resort
        /// PUT /api/admin/resorts/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ResortCreateUpdateDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data", errors = ModelState });

            var updated = await _resortService.UpdateAsync(id, dto);
            if (!updated)
                return NotFound(new { success = false, message = "Resort not found" });
            
            return Ok(new { success = true, message = "Resort updated successfully" });
        }

        /// <summary>
        /// Xóa resort
        /// DELETE /api/admin/resorts/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _resortService.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { success = false, message = "Resort not found" });
            
            return Ok(new { success = true, message = "Resort deleted successfully" });
        }
    }
}

