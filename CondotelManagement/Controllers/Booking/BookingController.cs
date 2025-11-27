using Microsoft.AspNetCore.Mvc;
using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Booking;
using CondotelManagement.Services.Interfaces.BookingService;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        private int GetCustomerId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyBookings()
        {
            int customerId = GetCustomerId();
            var bookings = await _bookingService.GetBookingsByCustomerAsync(customerId);
            return Ok(bookings);
        }

        // GET api/booking/{id}
        [HttpGet("{id}")]
        public IActionResult GetBookingById(int id)
        {
            var booking = _bookingService.GetBookingById(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }

        // GET api/booking/check-availability?condotelId=1&checkIn=2025-11-10&checkOut=2025-11-15
        [HttpGet("check-availability")]
        public IActionResult CheckAvailability(int condotelId, DateOnly checkIn, DateOnly checkOut)
        {
            bool available = _bookingService.CheckAvailability(condotelId, checkIn, checkOut);
            return Ok(new { condotelId, checkIn, checkOut, available });
        }

        // POST api/booking
        [HttpPost]
        public IActionResult CreateBooking([FromBody] BookingDTO dto)
        {
            dto.CustomerId = GetCustomerId();

            var result = _bookingService.CreateBooking(dto);

            if (!result.Success)
                return BadRequest(result);

            // Ép kiểu data về BookingDTO
            var booking = (BookingDTO)result.Data;

            return CreatedAtAction(nameof(GetBookingById), new { id = booking.BookingId }, result);
        }


        // PUT api/booking/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateBooking(int id, [FromBody] BookingDTO dto)
        {
            if (id != dto.BookingId) return BadRequest();
            var updated = _bookingService.UpdateBooking(dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        // POST api/booking/{id}/refund - Đặt trước DELETE để tránh route conflict
        [HttpPost("{id}/refund")]
        public async Task<IActionResult> RefundBooking(int id, [FromBody] RefundBookingRequestDTO? request = null)
        {
            int customerId = GetCustomerId();
            
            // Log request để debug
            Console.WriteLine($"[RefundBooking] BookingId: {id}, CustomerId: {customerId}");
            if (request != null)
            {
                Console.WriteLine($"[RefundBooking] BankCode: {request.BankCode}, AccountNumber: {request.AccountNumber}, AccountHolder: {request.AccountHolder}");
            }
            else
            {
                Console.WriteLine("[RefundBooking] Request body is null");
            }
            
            var result = await _bookingService.RefundBooking(id, customerId, request?.BankCode, request?.AccountNumber, request?.AccountHolder);
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // DELETE api/booking/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            int customerId = GetCustomerId();
            var success = await _bookingService.CancelBooking(id, customerId);
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
