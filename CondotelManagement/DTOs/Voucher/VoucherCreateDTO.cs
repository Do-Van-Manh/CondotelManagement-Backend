namespace CondotelManagement.DTOs
{
	public class VoucherCreateDTO
	{
		public int? CondotelID { get; set; }
		public int? UserID { get; set; }
		public string Code { get; set; } = null!;
		public decimal? DiscountAmount { get; set; }
		public decimal? DiscountPercentage { get; set; }
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
		public int? UsageLimit { get; set; }
	}
}
