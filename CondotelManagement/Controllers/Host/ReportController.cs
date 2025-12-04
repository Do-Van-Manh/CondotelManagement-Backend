using CondotelManagement.Helpers;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondotelManagement.Services.Interfaces;

namespace CondotelManagement.Controllers.Host
{
    [ApiController]
    [Route("api/host/[controller]")]
    [Authorize(Roles = "Host")]
    public class ReportController : ControllerBase
    {
        private readonly IHostReportService _service;
        private readonly IHostService _hostService;
        public ReportController(IHostReportService service, IHostService hostService)
        {
            _service = service;
            _hostService = hostService;
        }

        [HttpGet]
        public async Task<IActionResult> GetReport(
            [FromQuery] DateOnly? from,
            [FromQuery] DateOnly? to)
        {
            if (to < from)
                return BadRequest("Invalid date range: 'to' must be >= 'from'.");

            //current host login
            var host = _hostService.GetByUserId(User.GetUserId());
            if (host == null)
                return Unauthorized(new { message = "Host not found. Please register as a host first." });
            
            var hostId = host.HostId;
            var result = await _service.GetReport(hostId, from, to);
            return Ok(result);
        }

        // GET api/host/report/revenue?year=2024&month=1
        // Lấy báo cáo doanh thu theo tháng/năm
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenueReport(
            [FromQuery] int? year,
            [FromQuery] int? month)
        {
            // Validate month nếu có
            if (month.HasValue && (month < 1 || month > 12))
            {
                return BadRequest(new { message = "Month must be between 1 and 12" });
            }

            // Validate year nếu có
            if (year.HasValue && (year < 2000 || year > 2100))
            {
                return BadRequest(new { message = "Year must be between 2000 and 2100" });
            }

            // Validate: nếu có month thì phải có year
            if (month.HasValue && !year.HasValue)
            {
                return BadRequest(new { message = "Year is required when month is specified" });
            }

            // Lấy hostId từ user đang đăng nhập
            var host = _hostService.GetByUserId(User.GetUserId());
            if (host == null)
                return Unauthorized(new { message = "Host not found. Please register as a host first." });
            
            var hostId = host.HostId;
            var result = await _service.GetRevenueReportByMonthYear(hostId, year, month);
            
            // Debug: Log response structure
            Console.WriteLine($"[ReportController] Response - TotalRevenue: {result.TotalRevenue}, TotalBookings: {result.TotalBookings}");
            Console.WriteLine($"[ReportController] Response - MonthlyRevenues: {result.MonthlyRevenues?.Count ?? 0} items");
            Console.WriteLine($"[ReportController] Response - YearlyRevenues: {result.YearlyRevenues?.Count ?? 0} items");
            
            if (result.MonthlyRevenues != null && result.MonthlyRevenues.Any())
            {
                Console.WriteLine($"[ReportController] MonthlyRevenues details:");
                foreach (var m in result.MonthlyRevenues)
                {
                    Console.WriteLine($"  - {m.MonthName} {m.Year}: Revenue={m.Revenue}, Bookings={m.TotalBookings}, Completed={m.CompletedBookings}");
                }
            }
            
            return Ok(result);
        }
    }
}
