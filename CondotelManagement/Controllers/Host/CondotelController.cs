using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using CondotelManagement.Services.Interfaces;

namespace CondotelManagement.Controllers.Host
{
    [ApiController]
    [Route("api/host/[controller]")]
    [Authorize(Roles = "Host")]
    public class CondotelController : ControllerBase
    {
        private readonly ICondotelService _condotelService;
        private readonly IHostService _hostService;

        public CondotelController(ICondotelService condotelService, IHostService hostService)
        {
            _condotelService = condotelService;
            _hostService = hostService;
        }

        //GET /api/condotel
        [HttpGet]
        public ActionResult<IEnumerable<CondotelDTO>> GetAllCondotelByHost()
        {
            //current host login
            var host = _hostService.GetByUserId(User.GetUserId());
            if (host == null)
                return Unauthorized(new { message = "Host not found. Please register as a host first." });
            
            var hostId = host.HostId;
            var condotels = _condotelService.GetCondtelsByHost(hostId);
            return Ok(condotels);
        }

        //GET /api/condotel/{id}
        [HttpGet("{id}")]
        public ActionResult<CondotelDetailDTO> GetById(int id)
        {
            var condotel = _condotelService.GetCondotelById(id);
            if (condotel == null)
                return NotFound(new { message = "Condotel not found" });

            return Ok(condotel);
        }

        //POST /api/condotel
        [HttpPost]
        public ActionResult Create([FromBody] CondotelCreateDTO condotelDto)
        {
            if (condotelDto == null) 
                return BadRequest(new { message = "Invalid condotel data" });

            try
            {
                // Get current host from authenticated user
                var host = _hostService.GetByUserId(User.GetUserId());
                if (host == null)
                    return Unauthorized(new { message = "Host not found. Please register as a host first." });

                condotelDto.HostId = host.HostId;

                // Validate ownership - ensure host is creating for themselves
                var created = _condotelService.CreateCondotel(condotelDto);

                return CreatedAtAction(nameof(GetById),
                    new { id = created.CondotelId },
                    new { message = "Condotel created successfully", data = created });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating condotel", error = ex.Message });
            }
        }

        //PUT /api/condotel/{id}
        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] CondotelUpdateDTO condotelDto)
        {
            if (condotelDto == null)
                return BadRequest(new { message = "Invalid condotel data" });

            if (condotelDto.CondotelId != id)
                return BadRequest(new { message = "Condotel ID mismatch" });

            try
            {
                // Get current host from authenticated user
                var host = _hostService.GetByUserId(User.GetUserId());
                if (host == null)
                    return Unauthorized(new { message = "Host not found. Please register as a host first." });

                // Kiểm tra ownership - đảm bảo condotel thuộc về host này
                var existingCondotel = _condotelService.GetCondotelById(id);
                if (existingCondotel == null)
                    return NotFound(new { message = "Condotel not found" });

                if (existingCondotel.HostId != host.HostId)
                    return StatusCode(403, new { message = "You do not have permission to update this condotel" });

                // Set HostId từ authenticated user (không cho client set)
                condotelDto.HostId = host.HostId;

                var updated = _condotelService.UpdateCondotel(condotelDto);
                if (updated == null)
                    return NotFound(new { message = "Condotel not found" });

                return Ok(new { message = "Condotel updated successfully", data = updated });
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating condotel", error = ex.Message });
            }
        }

        //DELETE /api/condotel/{id}
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                // Get current host from authenticated user
                var host = _hostService.GetByUserId(User.GetUserId());
                if (host == null)
                    return Unauthorized(new { message = "Host not found. Please register as a host first." });

                // Kiểm tra ownership - đảm bảo condotel thuộc về host này
                var existingCondotel = _condotelService.GetCondotelById(id);
                if (existingCondotel == null)
                    return NotFound(new { message = "Condotel not found" });

                if (existingCondotel.HostId != host.HostId)
                    return StatusCode(403, new { message = "You do not have permission to delete this condotel" });

                var success = _condotelService.DeleteCondotel(id);
                if (!success)
                    return NotFound(new { message = "Condotel not found or already deleted" });

                return Ok(new { message = "Condotel deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting condotel", error = ex.Message });
            }
        }
    }
}
