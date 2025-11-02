namespace CondotelManagement.DTOs.Tenant
{
    public class RewardHistoryDTO
    {
        public int TransactionId { get; set; }
        public string Type { get; set; } // "Earned" hoặc "Redeemed"
        public int Points { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? RelatedBookingId { get; set; }
    }
}
