using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Repositories;
using CondotelManagement.Services;
using CondotelManagement.Services.Interfaces.BookingService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondotelManagement.Services.Interfaces;
using CondotelManagement.Models;

namespace CondotelManagement.Controllers.Host
{
    [ApiController]
    [Route("api/host/[controller]")]
    [Authorize(Roles = "Host")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly IHostService _hostService;

        public BookingController(IBookingService bookingService, IHostService hostService)
        {
            _bookingService = bookingService;
            _hostService = hostService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBookings()
        {
            //current host login
            var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
            var bookings = _bookingService.GetBookingsByHost(hostId);
            return Ok(ApiResponse<object>.SuccessResponse(bookings, null));
        }


        [HttpGet("customer/{customerId}")]
        public ActionResult<HostBookingDTO> GetBookingsByCustomer(int customerId)
        {
            //current host login
            var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
            var result = _bookingService.GetBookingsByHostAndCustomer(hostId, customerId);
            return Ok(ApiResponse<object>.SuccessResponse(result, null));
        }
    }
}
