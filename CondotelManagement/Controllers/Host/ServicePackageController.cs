using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Services;
using CondotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Host
{
    [ApiController]
    [Route("api/host/service-packages")]
    [Authorize(Roles = "Host")]
    public class ServicePackageController : ControllerBase
    {
        private readonly IServicePackageService _service;
		private readonly IHostService _hostService;

		public ServicePackageController(IServicePackageService service, IHostService hostService)
        {
            _service = service;
            _hostService = hostService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllByHost()
        {
			//current host login
			var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
			return Ok(await _service.GetAllByHostAsync(hostId));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateServicePackageDTO dto)
        {
            return Ok(await _service.CreateAsync(dto));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, UpdateServicePackageDTO dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _service.DeleteAsync(id);
            if (!success) return NotFound();
            return Ok(new { message = "Deleted successfully" });
        }
    }
}
