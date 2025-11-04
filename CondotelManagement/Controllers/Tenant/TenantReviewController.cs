using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondotelManagement.DTOs.Tenant;
using CondotelManagement.Services.Interfaces.Tenant;
using System.Security.Claims;


namespace CondotelManagement.Controllers.Tenant
{
    [ApiController]
    [Route("api/tenant/reviews")]
    [Authorize] // Yêu cầu đăng nhập
    public class TenantReviewController : ControllerBase
    {
        private readonly ITenantReviewService _reviewService;
        private readonly ILogger<TenantReviewController> _logger;

        public TenantReviewController(
            ITenantReviewService reviewService,
            ILogger<TenantReviewController> logger)
        {
            _reviewService = reviewService;
            _logger = logger;
        }

        /// <summary>
        /// Tạo review mới cho booking đã hoàn thành
        /// POST /api/tenant/reviews
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] ReviewDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Kiểm tra có thể review không
                var canReview = await _reviewService.CanReviewBookingAsync(userId, dto.BookingId);
                if (!canReview.CanReview)
                {
                    return BadRequest(new { message = canReview.Message });
                }

                var review = await _reviewService.CreateReviewAsync(userId, dto);

                return CreatedAtAction(
                    nameof(GetReviewById),
                    new { id = review.ReviewId },
                    new
                    {
                        success = true,
                        message = "Review created successfully",
                        data = review
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating review");
                return StatusCode(500, new { message = "An error occurred while creating review" });
            }
        }

        /// <summary>
        /// Lấy danh sách review của tôi
        /// GET /api/tenant/reviews
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyReviews([FromQuery] ReviewQueryDTO query)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (reviews, totalCount) = await _reviewService.GetMyReviewsAsync(userId, query);

                return Ok(new
                {
                    success = true,
                    data = reviews,
                    pagination = new
                    {
                        page = query.Page,
                        pageSize = query.PageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reviews");
                return StatusCode(500, new { message = "An error occurred while getting reviews" });
            }
        }

        /// <summary>
        /// Lấy chi tiết 1 review
        /// GET /api/tenant/reviews/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var review = await _reviewService.GetReviewByIdAsync(id, userId);

                if (review == null)
                {
                    return NotFound(new { message = "Review not found" });
                }

                return Ok(new { success = true, data = review });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting review {id}");
                return StatusCode(500, new { message = "An error occurred while getting review" });
            }
        }

        /// <summary>
        /// Cập nhật review (trong vòng 7 ngày)
        /// PUT /api/tenant/reviews/{id}
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDTO dto)
        {
            try
            {
                if (id != dto.ReviewId)
                {
                    return BadRequest(new { message = "Review ID mismatch" });
                }

                var userId = GetCurrentUserId();
                var review = await _reviewService.UpdateReviewAsync(userId, dto);

                return Ok(new
                {
                    success = true,
                    message = "Review updated successfully",
                    data = review
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating review {id}");
                return StatusCode(500, new { message = "An error occurred while updating review" });
            }
        }

        /// <summary>
        /// Xóa review (trong vòng 7 ngày)
        /// DELETE /api/tenant/reviews/{id}
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                var deleted = await _reviewService.DeleteReviewAsync(id, userId);

                if (!deleted)
                {
                    return NotFound(new { message = "Review not found" });
                }

                return Ok(new
                {
                    success = true,
                    message = "Review deleted successfully"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting review {id}");
                return StatusCode(500, new { message = "An error occurred while deleting review" });
            }
        }

        /// <summary>
        /// Kiểm tra có thể review booking không
        /// GET /api/tenant/reviews/can-review/{bookingId}
        /// </summary>
        [HttpGet("can-review/{bookingId}")]
        public async Task<IActionResult> CanReviewBooking(int bookingId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (canReview, message) = await _reviewService.CanReviewBookingAsync(userId, bookingId);

                return Ok(new
                {
                    canReview,
                    message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking can review booking {bookingId}");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // Helper method để lấy UserId từ JWT token
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }
            return userId;
        }
    }
}