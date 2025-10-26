using CondotelManagement.DTOs;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CondotelManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CondotelController : ControllerBase
    {
        private readonly ICondotelService _condotelService;

        public CondotelController(ICondotelService condotelService)
        {
            _condotelService = condotelService;
        }

        //GET /api/condotel
        [HttpGet]
        public ActionResult<IEnumerable<CondotelDTO>> GetAll()
        {
            var condotels = _condotelService.GetCondotels();
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
        public ActionResult Create([FromBody] CondotelDetailDTO condotelDto)
        {
            if (condotelDto == null) return BadRequest("Invalid condotel data");
            var created = _condotelService.CreateCondotel(condotelDto);
            return CreatedAtAction(nameof(GetById),
                new { id = condotelDto.CondotelId },
                created);
        }

        //PUT /api/condotel/{id}
        [HttpPut("{id}")]
        public ActionResult Update(int id, [FromBody] CondotelDetailDTO condotelDto)
        {
            if (condotelDto == null || condotelDto.CondotelId != id) return BadRequest();
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
