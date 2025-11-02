using Microsoft.AspNetCore.Mvc;
using CondotelManagement.Services;
using CondotelManagement.DTOs;
using CondotelManagement.Services.Interfaces.BookingService;
using Microsoft.AspNetCore.Authorization;

namespace CondotelManagement.Controllers
{
    [ApiController]
    [Authorize(Roles = "Tenant")]
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
        // GET api/booking/check-availability?roomId=1&checkIn=2025-10-28&checkOut=2025-10-30
        [HttpGet("check-availability")]
        public IActionResult CheckAvailability(int roomId, DateOnly checkIn, DateOnly checkOut)
        {
            bool isAvailable = _bookingService.CheckAvailability(roomId, checkIn, checkOut);
            return Ok(new
            {
                roomId,
                checkIn,
                checkOut,
                available = isAvailable
            });
        }

        // POST api/booking
        [HttpPost]
        public IActionResult CreateBooking([FromBody] BookingDTO dto)
        {
            if (dto == null) return BadRequest("Invalid booking data");
            var created = _bookingService.CreateBooking(dto);
            return CreatedAtAction(nameof(GetBookingById), new { id = created.BookingId }, created);
        }
        //hủy đặt phòng
        // DELETE api/booking/5
        [HttpDelete("{id}")]
        public IActionResult CancelBooking(int id)
        {
            var canceled = _bookingService.CancelBooking(id);
            if (!canceled) return NotFound();
            return NoContent();
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

       
    }
}
