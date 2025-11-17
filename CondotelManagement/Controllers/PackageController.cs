using CondotelManagement.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly IPackageService _packageService;

        public PackageController(IPackageService packageService)
        {
            _packageService = packageService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailablePackages()
        {
            var packages = await _packageService.GetAvailablePackagesAsync();
            return Ok(packages);
        }
    }
}