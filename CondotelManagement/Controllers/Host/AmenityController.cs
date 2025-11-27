using CondotelManagement.DTOs.Amenity;
using CondotelManagement.Services.Interfaces.Amenity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Host
{
    [ApiController]
    [Route("api/host/amenities")]
    [Authorize(Roles = "Host")]
    public class HostAmenityController : ControllerBase
    {
        private readonly IAmenityService _amenityService;

        public HostAmenityController(IAmenityService amenityService)
        {
            _amenityService = amenityService;
        }

        // GET: /api/host/amenities
        // Lấy tất cả amenities
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var amenities = await _amenityService.GetAllAsync();
                return Ok(amenities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching amenities", error = ex.Message });
            }
        }

        // GET: /api/host/amenities/{id}
        // Lấy amenity theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var amenity = await _amenityService.GetByIdAsync(id);
                if (amenity == null)
                    return NotFound(new { message = "Amenity not found" });

                return Ok(amenity);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching amenity", error = ex.Message });
            }
        }

        // GET: /api/host/amenities/by-category/{category}
        // Lấy amenities theo category
        [HttpGet("by-category/{category}")]
        public async Task<IActionResult> GetByCategory(string category)
        {
            try
            {
                var allAmenities = await _amenityService.GetAllAsync();
                var filtered = allAmenities.Where(a => 
                    !string.IsNullOrWhiteSpace(a.Category) && 
                    a.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
                
                return Ok(filtered);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching amenities by category", error = ex.Message });
            }
        }

        // POST: /api/host/amenities
        // Tạo amenity mới
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AmenityRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var created = await _amenityService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.AmenityId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating amenity", error = ex.Message });
            }
        }

        // PUT: /api/host/amenities/{id}
        // Cập nhật amenity
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AmenityRequestDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var success = await _amenityService.UpdateAsync(id, dto);
                if (!success)
                    return NotFound(new { message = "Amenity not found" });

                return Ok(new { message = "Amenity updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating amenity", error = ex.Message });
            }
        }

        // DELETE: /api/host/amenities/{id}
        // Xóa amenity
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _amenityService.DeleteAsync(id);
                if (!success)
                    return NotFound(new { message = "Amenity not found or cannot be deleted" });

                return Ok(new { message = "Amenity deleted successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting amenity", error = ex.Message });
            }
        }
    }
}
