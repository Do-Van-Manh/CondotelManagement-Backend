using CondotelManagement.DTOs.Tenant;

namespace CondotelManagement.Services.Interfaces.Tenant
{
    public interface ITenantRewardService
    {
        /// <summary>
        /// Lấy thông tin điểm thưởng hiện tại
        /// </summary>
        Task<RewardPointsDTO?> GetMyPointsAsync(int userId);

        /// <summary>
        /// Lấy lịch sử giao dịch điểm thưởng
        /// </summary>
        Task<(List<RewardHistoryDTO> History, int TotalCount)> GetPointsHistoryAsync(int userId, RewardHistoryQueryDTO query);

        /// <summary>
        /// Đổi điểm thưởng để giảm giá booking
        /// </summary>
        Task<RedeemResponseDTO> RedeemPointsAsync(int userId, RedeemPointsDTO dto);

        /// <summary>
        /// Tính điểm thưởng khi hoàn thành booking
        /// </summary>
        Task<int> EarnPointsFromBookingAsync(int userId, int bookingId);

        /// <summary>
        /// Lấy danh sách promotion có thể dùng
        /// </summary>
        Task<List<AvailablePromotionDTO>> GetAvailablePromotionsAsync(int userId);

        /// <summary>
        /// Tính số tiền giảm giá từ điểm thưởng
        /// </summary>
        decimal CalculateDiscountFromPoints(int points);

        /// <summary>
        /// Kiểm tra có đủ điểm để đổi không
        /// </summary>
        Task<(bool IsValid, string Message)> ValidateRedeemPointsAsync(int userId, int pointsToRedeem);
    }
}
