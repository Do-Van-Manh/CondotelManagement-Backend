using CondotelManagement.Helpers;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
            var result = await _service.GetReport(hostId, from, to);
            return Ok(result);
        }
    }
}
