namespace CondotelManagement.DTOs.Tenant
{
    public class RedeemResponseDTO
    {
        public bool Success { get; set; }
        public int PointsRedeemed { get; set; }
        public decimal DiscountAmount { get; set; }
        public int RemainingPoints { get; set; }
        public string Message { get; set; }
    }

}
