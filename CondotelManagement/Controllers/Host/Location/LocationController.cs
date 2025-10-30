using CondotelManagement.DTOs;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Host.Location
{
    [ApiController]
    [Route("api/host/[controller]")]
    [Authorize(Roles = "Host")]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationDTO>>> GetAll()
        {
            var locations = await _locationService.GetAllAsync();
            return Ok(locations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LocationDTO>> GetById(int id)
        {
            var location = await _locationService.GetByIdAsync(id);
            if (location == null) return NotFound();
            return Ok(location);
        }

        [HttpPost]
        public async Task<ActionResult<LocationDTO>> Create(LocationCreateUpdateDTO dto)
        {
            var created = await _locationService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.LocationId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, LocationCreateUpdateDTO dto)
        {
            var updated = await _locationService.UpdateAsync(id, dto);
            if (!updated) return NotFound();
            return Ok(new { message = "Location updated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _locationService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return Ok(new { message = "Location deleted successfully" });
        }
    }
}
