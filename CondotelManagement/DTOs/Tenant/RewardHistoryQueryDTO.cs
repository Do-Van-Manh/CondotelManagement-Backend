namespace CondotelManagement.DTOs.Tenant
{
    public class RewardHistoryQueryDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Type { get; set; } // "Earned", "Redeemed", hoặc null (all)
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
