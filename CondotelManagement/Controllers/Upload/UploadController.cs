using CondotelManagement.Services.Interfaces.Cloudinary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Upload
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ICloudinaryService _cloud;

        public UploadController(ICloudinaryService cloud) => _cloud = cloud;

        [HttpPost("image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null) return BadRequest("No file uploaded");
            var url = await _cloud.UploadImageAsync(file);
            return Ok(new { imageUrl = url });
        }
    }
}
