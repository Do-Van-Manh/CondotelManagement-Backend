using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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
            var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
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
                    return Unauthorized(new { message = "Host not found or unauthorized" });

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
            if (condotelDto == null || condotelDto.CondotelId != id) return BadRequest();
            //current host login
            var host = _hostService.GetByUserId(User.GetUserId());
            condotelDto.HostId = host.HostId;
            var updated = _condotelService.UpdateCondotel(condotelDto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        //DELETE /api/condotel/{id}
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var success = _condotelService.DeleteCondotel(id);
            if (!success) return NotFound();
            return Ok(new { message = "Condotel deleted successfully" });
        }
    }
}
