using CondotelManagement.DTOs.Blog;
using CondotelManagement.Services.Interfaces.Blog; // Sửa Using
using CondotelManagement.Services.Interfaces.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Admin
{
    [Route("api/admin/blog")]
    [ApiController]
    [Authorize(Roles = "Admin, ContentManager")]
    public class AdminBlogController : ControllerBase
    {
        private readonly IBlogService _blogService;
        private readonly IAuthService _authService;

        public AdminBlogController(IBlogService blogService, IAuthService authService)
        {
            _blogService = blogService;
            _authService = authService;
        }

        // SỬA LẠI: Hàm GetById cho Admin
        // Endpoint này dùng để trả về 201 Created
        [HttpGet("posts/{postId}", Name = "AdminGetPostById")] // Đặt tên Route
        public async Task<IActionResult> AdminGetPostById(int postId)
        {
            var post = await _blogService.AdminGetPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết." });
            }
            return Ok(post);
        }

        [HttpPost("posts")]
        public async Task<IActionResult> CreatePost([FromBody] AdminBlogCreateDto dto)
        {
            var user = await _authService.GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var result = await _blogService.AdminCreatePostAsync(dto, user.UserId);
            if (result == null)
            {
                return BadRequest(new { message = "Không thể tạo bài viết." });
            }

            // SỬA LẠI: Trỏ đúng tên Route
            return CreatedAtRoute("AdminGetPostById", new { postId = result.PostId }, result);
        }

        [HttpPut("posts/{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, [FromBody] AdminBlogCreateDto dto)
        {
            var result = await _blogService.AdminUpdatePostAsync(postId, dto);
            if (result == null)
            {
                return NotFound(new { message = "Không tìm thấy bài viết." });
            }
            return Ok(result);
        }

        [HttpDelete("posts/{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var success = await _blogService.AdminDeletePostAsync(postId);
            if (!success)
            {
                return NotFound(new { message = "Không tìm thấy bài viết." });
            }
            return Ok(new { message = "Xóa bài viết thành công." });
        }
    }
}