using CondotelManagement.Data;
using CondotelManagement.DTOs.Tenant;
using CondotelManagement.Models;
using CondotelManagement.Services.Interfaces.Tenant;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Services.Implementations.Tenant
{
    public class TenantRewardService : ITenantRewardService
    {
        private readonly CondotelDbVer1Context _context;
        private readonly ILogger<TenantRewardService> _logger;

        // Business rules
        private const decimal POINTS_TO_MONEY_RATE = 1000m; // 1000 điểm = 1 đơn vị tiền tệ
        private const int MIN_POINTS_TO_REDEEM = 1000; // Tối thiểu 1000 điểm mới đổi được
        private const int POINTS_PER_BOOKING_PERCENT = 1; // Tích 1% giá trị booking thành điểm
        private const int POINTS_PER_REVIEW = 100; // Thưởng 100 điểm khi review

        public TenantRewardService(CondotelDbVer1Context context, ILogger<TenantRewardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<RewardPointsDTO?> GetMyPointsAsync(int userId)
        {
            var rewardPoints = await _context.RewardPoints
                .FirstOrDefaultAsync(rp => rp.CustomerId == userId);

            if (rewardPoints == null)
            {
                // Tạo mới nếu chưa có
                rewardPoints = new RewardPoint
                {
                    CustomerId = userId,
                    Points = 0,
                    LastUpdated = DateTime.Now
                };
                _context.RewardPoints.Add(rewardPoints);
                await _context.SaveChangesAsync();
            }

            return new RewardPointsDTO
            {
                PointId = rewardPoints.PointId,
                CustomerId = rewardPoints.CustomerId,
                TotalPoints = rewardPoints.Points,
                LastUpdated = rewardPoints.LastUpdated,
                PointsExpiringSoon = 0, // TODO: Implement expiry logic
                Tier = GetTierByPoints(rewardPoints.Points)
            };
        }

        public async Task<(List<RewardHistoryDTO> History, int TotalCount)> GetPointsHistoryAsync(int userId, RewardHistoryQueryDTO query)
        {
            // TODO: Cần tạo bảng RewardTransactions để lưu lịch sử
            // Hiện tại trả về empty list
            _logger.LogWarning("RewardTransactions table not implemented yet");

            return (new List<RewardHistoryDTO>(), 0);

            /* 
            // Implementation khi có bảng RewardTransactions:
            var historyQuery = _context.RewardTransactions
                .Where(rt => rt.CustomerId == userId);

            if (!string.IsNullOrEmpty(query.Type))
            {
                historyQuery = historyQuery.Where(rt => rt.Type == query.Type);
            }

            if (query.FromDate.HasValue)
            {
                historyQuery = historyQuery.Where(rt => rt.CreatedAt >= query.FromDate.Value);
            }

            if (query.ToDate.HasValue)
            {
                historyQuery = historyQuery.Where(rt => rt.CreatedAt <= query.ToDate.Value);
            }

            var totalCount = await historyQuery.CountAsync();

            var history = await historyQuery
                .OrderByDescending(rt => rt.CreatedAt)
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(rt => new RewardHistoryDTO
                {
                    TransactionId = rt.TransactionId,
                    Type = rt.Type,
                    Points = rt.Points,
                    Description = rt.Description,
                    CreatedAt = rt.CreatedAt,
                    RelatedBookingId = rt.BookingId
                })
                .ToListAsync();

            return (history, totalCount);
            */
        }

        public async Task<RedeemResponseDTO> RedeemPointsAsync(int userId, RedeemPointsDTO dto)
        {
            // 1. Validate booking tồn tại và thuộc về user
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == dto.BookingId && b.CustomerId == userId);

            if (booking == null)
            {
                throw new InvalidOperationException("Booking not found or does not belong to you");
            }

            // 2. Kiểm tra trạng thái booking (chỉ được dùng điểm cho booking Pending)
            if (booking.Status != "Pending")
            {
                throw new InvalidOperationException("You can only use reward points for pending bookings");
            }

            // 3. Kiểm tra đã dùng điểm cho booking này chưa
            if (booking.IsUsingRewardPoints)
            {
                throw new InvalidOperationException("Reward points have already been applied to this booking");
            }

            // 4. Validate số điểm
            var validation = await ValidateRedeemPointsAsync(userId, dto.PointsToRedeem);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException(validation.Message);
            }

            // 5. Tính số tiền giảm giá
            var discountAmount = CalculateDiscountFromPoints(dto.PointsToRedeem);

            // 6. Kiểm tra discount không vượt quá giá booking
            if (discountAmount > booking.TotalPrice)
            {
                throw new InvalidOperationException($"Discount amount ({discountAmount}) cannot exceed booking price ({booking.TotalPrice})");
            }

            // 7. Trừ điểm
            var rewardPoints = await _context.RewardPoints
                .FirstOrDefaultAsync(rp => rp.CustomerId == userId);

            if (rewardPoints == null)
            {
                throw new InvalidOperationException("Reward points record not found");
            }

            rewardPoints.Points -= dto.PointsToRedeem;
            rewardPoints.LastUpdated = DateTime.Now;

            // 8. Cập nhật booking
            booking.IsUsingRewardPoints = true;
            booking.TotalPrice -= discountAmount;

            // 9. TODO: Lưu transaction history
            // _context.RewardTransactions.Add(new RewardTransaction { ... });

            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} redeemed {dto.PointsToRedeem} points for booking {dto.BookingId}. Discount: {discountAmount}");

            return new RedeemResponseDTO
            {
                Success = true,
                PointsRedeemed = dto.PointsToRedeem,
                DiscountAmount = discountAmount,
                RemainingPoints = rewardPoints.Points,
                Message = $"Successfully redeemed {dto.PointsToRedeem} points for ${discountAmount} discount"
            };
        }

        public async Task<int> EarnPointsFromBookingAsync(int userId, int bookingId)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == bookingId && b.CustomerId == userId);

            if (booking == null)
            {
                throw new InvalidOperationException("Booking not found");
            }

            // Chỉ tích điểm cho booking Completed
            if (booking.Status != "Completed")
            {
                throw new InvalidOperationException("Only completed bookings earn reward points");
            }

            // Tính điểm = 1% giá trị booking (làm tròn)
            var pointsEarned = (int)(booking.TotalPrice * POINTS_PER_BOOKING_PERCENT / 100);

            // Cập nhật điểm
            var rewardPoints = await _context.RewardPoints
                .FirstOrDefaultAsync(rp => rp.CustomerId == userId);

            if (rewardPoints == null)
            {
                rewardPoints = new RewardPoint
                {
                    CustomerId = userId,
                    Points = 0,
                    LastUpdated = DateTime.Now
                };
                _context.RewardPoints.Add(rewardPoints);
            }

            rewardPoints.Points += pointsEarned;
            rewardPoints.LastUpdated = DateTime.Now;

            // TODO: Lưu transaction history
            // _context.RewardTransactions.Add(new RewardTransaction { ... });

            await _context.SaveChangesAsync();

            _logger.LogInformation($"User {userId} earned {pointsEarned} points from booking {bookingId}");

            return pointsEarned;
        }

        public async Task<List<AvailablePromotionDTO>> GetAvailablePromotionsAsync(int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            var promotions = await _context.Promotions
                .Where(p => p.Status == "Active"
                    && p.StartDate <= today
                    && p.EndDate >= today)
                .ToListAsync();

            return promotions.Select(p => new AvailablePromotionDTO
            {
                PromotionId = p.PromotionId,
                Name = p.Name,
                StartDate = p.StartDate.ToDateTime(TimeOnly.MinValue),
                EndDate = p.EndDate.ToDateTime(TimeOnly.MinValue),
                DiscountPercentage = p.DiscountPercentage,
                TargetAudience = p.TargetAudience ?? "All",
                IsApplicable = IsPromotionApplicableToUser(p, userId),
                DaysRemaining = (p.EndDate.ToDateTime(TimeOnly.MinValue) - DateTime.Now).Days
            }).ToList();
        }

        public decimal CalculateDiscountFromPoints(int points)
        {
            // 1000 điểm = 1 đơn vị tiền
            return points / POINTS_TO_MONEY_RATE;
        }

        public async Task<(bool IsValid, string Message)> ValidateRedeemPointsAsync(int userId, int pointsToRedeem)
        {
            // 1. Kiểm tra số điểm tối thiểu
            if (pointsToRedeem < MIN_POINTS_TO_REDEEM)
            {
                return (false, $"Minimum {MIN_POINTS_TO_REDEEM} points required to redeem");
            }

            // 2. Kiểm tra điểm hiện có
            var rewardPoints = await _context.RewardPoints
                .FirstOrDefaultAsync(rp => rp.CustomerId == userId);

            if (rewardPoints == null || rewardPoints.Points < pointsToRedeem)
            {
                return (false, $"Insufficient points. You have {rewardPoints?.Points ?? 0} points");
            }

            // 3. Kiểm tra điểm phải là bội số của 1000
            if (pointsToRedeem % 1000 != 0)
            {
                return (false, "Points must be a multiple of 1000");
            }

            return (true, "Valid");
        }

        // Helper methods
        private string GetTierByPoints(int points)
        {
            if (points >= 10000) return "Gold";
            if (points >= 5000) return "Silver";
            return "Bronze";
        }

        private bool IsPromotionApplicableToUser(Promotion promotion, int userId)
        {
            // TODO: Implement logic kiểm tra target audience
            // Ví dụ: "New Customer", "VIP Customer", "All"

            if (string.IsNullOrEmpty(promotion.TargetAudience) || promotion.TargetAudience == "All")
            {
                return true;
            }

            // Thêm logic check các điều kiện khác
            return true;
        }
    }

}
