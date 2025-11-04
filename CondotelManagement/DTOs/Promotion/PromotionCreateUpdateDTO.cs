namespace CondotelManagement.DTOs
{
    public class PromotionCreateUpdateDTO
    {
        public string Name { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal DiscountPercentage { get; set; }
        public string? TargetAudience { get; set; }
        public string Status { get; set; } = "Active";
        public int? CondotelId { get; set; }
    }
}



