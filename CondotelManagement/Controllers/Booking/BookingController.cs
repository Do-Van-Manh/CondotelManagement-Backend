using Microsoft.AspNetCore.Mvc;
using CondotelManagement.Services;
using CondotelManagement.DTOs;
using CondotelManagement.Services.Interfaces.BookingService;

namespace CondotelManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        // GET api/booking/customer/5
        [HttpGet("customer/{customerId}")]
        public IActionResult GetBookingsByCustomer(int customerId)
        {
            var bookings = _bookingService.GetBookingsByCustomer(customerId);
            return Ok(bookings);
        }

        // GET api/booking/5
        [HttpGet("{id}")]
        public IActionResult GetBookingById(int id)
        {
            var booking = _bookingService.GetBookingById(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        // POST api/booking
        [HttpPost]
        public IActionResult CreateBooking([FromBody] BookingDTO dto)
        {
            if (dto == null) return BadRequest("Invalid booking data");
            var created = _bookingService.CreateBooking(dto);
            return CreatedAtAction(nameof(GetBookingById), new { id = created.BookingId }, created);
        }

        // PUT api/booking/5
        [HttpPut("{id}")]
        public IActionResult UpdateBooking(int id, [FromBody] BookingDTO dto)
        {
            if (dto == null || dto.BookingId != id) return BadRequest();
            var updated = _bookingService.UpdateBooking(dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // DELETE api/booking/5
        [HttpDelete("{id}")]
        public IActionResult DeleteBooking(int id)
        {
            var success = _bookingService.DeleteBooking(id);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
