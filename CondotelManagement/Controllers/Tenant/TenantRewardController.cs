using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CondotelManagement.DTOs.Tenant;
using System.Security.Claims;
using CondotelManagement.Services.Interfaces.Tenant;

namespace CondotelManagement.Controllers.Tenant
{
    [ApiController]
    [Route("api/tenant/rewards")]
    [Authorize] // Yêu cầu đăng nhập
    public class TenantRewardController : ControllerBase
    {
        private readonly ITenantRewardService _rewardService;
        private readonly ILogger<TenantRewardController> _logger;

        public TenantRewardController(
            ITenantRewardService rewardService,
            ILogger<TenantRewardController> logger)
        {
            _rewardService = rewardService;
            _logger = logger;
        }

        /// <summary>
        /// Xem số điểm thưởng hiện tại của tôi
        /// GET /api/tenant/rewards/points
        /// </summary>
        [HttpGet("points")]
        public async Task<IActionResult> GetMyPoints()
        {
            try
            {
                var userId = GetCurrentUserId();
                var points = await _rewardService.GetMyPointsAsync(userId);

                if (points == null)
                {
                    return NotFound(new { message = "Reward points not found" });
                }

                return Ok(new
                {
                    success = true,
                    data = points,
                    info = new
                    {
                        pointsToMoneyRate = "1000 points = $1",
                        minPointsToRedeem = 1000,
                        currentValue = $"${_rewardService.CalculateDiscountFromPoints(points.TotalPoints)}"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reward points");
                return StatusCode(500, new { message = "An error occurred while getting points" });
            }
        }

        /// <summary>
        /// Xem lịch sử tích điểm/dùng điểm
        /// GET /api/tenant/rewards/history
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetPointsHistory([FromQuery] RewardHistoryQueryDTO query)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (history, totalCount) = await _rewardService.GetPointsHistoryAsync(userId, query);

                return Ok(new
                {
                    success = true,
                    data = history,
                    pagination = new
                    {
                        page = query.Page,
                        pageSize = query.PageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
                    },
                    note = "History feature requires RewardTransactions table to be implemented"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting points history");
                return StatusCode(500, new { message = "An error occurred while getting history" });
            }
        }

        /// <summary>
        /// Đổi điểm thưởng để giảm giá booking
        /// POST /api/tenant/rewards/redeem
        /// </summary>
        [HttpPost("redeem")]
        public async Task<IActionResult> RedeemPoints([FromBody] RedeemPointsDTO dto)
        {
            try
            {
                var userId = GetCurrentUserId();

                // Validate trước
                var validation = await _rewardService.ValidateRedeemPointsAsync(userId, dto.PointsToRedeem);
                if (!validation.IsValid)
                {
                    return BadRequest(new { message = validation.Message });
                }

                var result = await _rewardService.RedeemPointsAsync(userId, dto);

                return Ok(new
                {
                    success = result.Success,
                    message = result.Message,
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redeeming points");
                return StatusCode(500, new { message = "An error occurred while redeeming points" });
            }
        }

        /// <summary>
        /// Xem các chương trình khuyến mãi đang có
        /// GET /api/tenant/rewards/promotions
        /// </summary>
        [HttpGet("promotions")]
        public async Task<IActionResult> GetAvailablePromotions()
        {
            try
            {
                var userId = GetCurrentUserId();
                var promotions = await _rewardService.GetAvailablePromotionsAsync(userId);

                return Ok(new
                {
                    success = true,
                    data = promotions,
                    count = promotions.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting promotions");
                return StatusCode(500, new { message = "An error occurred while getting promotions" });
            }
        }

        /// <summary>
        /// Tính toán số tiền giảm giá từ điểm
        /// GET /api/tenant/rewards/calculate-discount?points=5000
        /// </summary>
        [HttpGet("calculate-discount")]
        public IActionResult CalculateDiscount([FromQuery] int points)
        {
            try
            {
                if (points < 0)
                {
                    return BadRequest(new { message = "Points must be positive" });
                }

                var discount = _rewardService.CalculateDiscountFromPoints(points);

                return Ok(new
                {
                    success = true,
                    points,
                    discountAmount = discount,
                    formula = "1000 points = $1"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating discount");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// Validate có thể đổi điểm không
        /// GET /api/tenant/rewards/validate-redeem?points=5000
        /// </summary>
        [HttpGet("validate-redeem")]
        public async Task<IActionResult> ValidateRedeem([FromQuery] int points)
        {
            try
            {
                var userId = GetCurrentUserId();
                var (isValid, message) = await _rewardService.ValidateRedeemPointsAsync(userId, points);

                return Ok(new
                {
                    isValid,
                    message,
                    discountAmount = isValid ? _rewardService.CalculateDiscountFromPoints(points) : 0
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating redeem");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        /// <summary>
        /// [INTERNAL] Tích điểm khi hoàn thành booking (gọi từ system)
        /// POST /api/tenant/rewards/earn-from-booking/{bookingId}
        /// </summary>
        [HttpPost("earn-from-booking/{bookingId}")]
        [Authorize(Roles = "Admin")] // Chỉ admin/system mới gọi được
        public async Task<IActionResult> EarnPointsFromBooking(int bookingId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var pointsEarned = await _rewardService.EarnPointsFromBookingAsync(userId, bookingId);

                return Ok(new
                {
                    success = true,
                    message = $"Earned {pointsEarned} points from booking",
                    pointsEarned
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error earning points from booking");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }

        // Helper method
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