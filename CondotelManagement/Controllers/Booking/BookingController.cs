using Microsoft.AspNetCore.Mvc;
using CondotelManagement.DTOs;
using CondotelManagement.DTOs.Booking;
using CondotelManagement.Services.Interfaces.BookingService;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CondotelManagement.Controllers
{
    [ApiController]
    [Authorize]
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
        public async Task<IActionResult> GetBookingById(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
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
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDTO dto)
        {
           
            var customerId = GetCustomerId();
            if (customerId <= 0)
                return Unauthorized("Không tìm thấy thông tin user.");

            var result = await _bookingService.CreateBookingAsync(dto, customerId);

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
            // 1. Validate null body (phải làm trước)
            if (dto == null)
                return BadRequest("Request body trống.");

            // 2. Validate id trùng với body
            if (id != dto.BookingId)
                return BadRequest("Booking ID không khớp với URL.");

            // 3. Validate logic nghiệp vụ nhẹ
            if (dto.StartDate > dto.EndDate)
                return BadRequest("Ngày bắt đầu phải trước ngày kết thúc.");

            // 4. Validate không cho sửa các field nhạy cảm
            if (dto.CustomerId <= 0 || dto.CondotelId <= 0)
                return BadRequest("Không được chỉnh sửa customerId hoặc condotelId.");

            try
            {
                var updated = _bookingService.UpdateBooking(dto);

                if (updated == null)
                    return NotFound("Không tìm thấy booking.");

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                // Các lỗi nghiệp vụ: Completed, Cancelled, đã check-in, status sai...
                return BadRequest(ex.Message);
            }
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
            
            // Kiểm tra booking có tồn tại và thuộc về user không
            var booking = await _bookingService.GetBookingByIdAsync(id);
            if (booking == null || booking.CustomerId != customerId)
            {
                return NotFound(new { success = false, message = "Booking not found or you don't have permission to cancel this booking." });
            }
            
            // Nếu booking đã thanh toán, thử refund trước
            if (booking.Status == "Confirmed" || booking.Status == "Completed")
            {
                // Kiểm tra có thể refund không
                var canRefund = await _bookingService.CanRefundBooking(id, customerId);
                if (!canRefund)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = "Cannot cancel this booking. It may not be eligible for refund (e.g., too close to check-in date, already paid to host, or outside refund window)." 
                    });
                }
                
                // Thử refund
                var refundResult = await _bookingService.RefundBooking(id, customerId);
                if (!refundResult.Success)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = refundResult.Message ?? "Failed to process refund. Please try again or contact support." 
                    });
                }
                
                return Ok(new { 
                    success = true, 
                    message = "Booking cancelled and refund request created successfully.",
                    data = refundResult.Data
                });
            }
            
            // Nếu booking chưa thanh toán, chỉ cần cancel
            var success = await _bookingService.CancelBooking(id, customerId);
            if (!success)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Failed to cancel booking. Please try again or contact support." 
                });
            }
            
            return Ok(new { 
                success = true, 
                message = "Booking cancelled successfully." 
            });
        }

        // POST api/booking/{id}/cancel-payment - Hủy thanh toán (KHÔNG refund)
        [HttpPost("{id}/cancel-payment")]
        public async Task<IActionResult> CancelPayment(int id)
        {
            int customerId = GetCustomerId();
            var success = await _bookingService.CancelPayment(id, customerId);
            if (!success) 
                return BadRequest(new { success = false, message = "Cannot cancel payment. Booking may have already been paid or cancelled." });
            return Ok(new { success = true, message = "Payment cancelled successfully. Booking status updated to Cancelled." });
        }

        /// <summary>
        /// Kiểm tra xem booking có thể hoàn tiền được không (để hiển thị nút hoàn tiền)
        /// </summary>
        [HttpGet("{id}/can-refund")]
        public async Task<IActionResult> CanRefundBooking(int id)
        {
            int customerId = GetCustomerId();
            var canRefund = await _bookingService.CanRefundBooking(id, customerId);
            
            return Ok(new 
            { 
                success = true,
                canRefund = canRefund,
                message = canRefund 
                    ? "Booking can be refunded. Refund button should be displayed." 
                    : "Booking cannot be refunded. Refund button should be hidden."
            });
        }
    }
}
