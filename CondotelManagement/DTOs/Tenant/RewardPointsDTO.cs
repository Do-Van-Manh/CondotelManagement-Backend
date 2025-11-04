namespace CondotelManagement.DTOs.Tenant
{
    public class RewardPointsDTO
    {
        public int PointId { get; set; }
        public int CustomerId { get; set; }
        public int TotalPoints { get; set; }
        public DateTime LastUpdated { get; set; }
        public int PointsExpiringSoon { get; set; } // Điểm sắp hết hạn (nếu có logic)
        public string Tier { get; set; } // Bronze/Silver/Gold dựa vào điểm
    }
}
