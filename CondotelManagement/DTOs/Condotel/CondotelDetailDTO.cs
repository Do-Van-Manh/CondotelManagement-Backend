namespace CondotelManagement.DTOs
{
    public class CondotelDetailDTO
    {
        public int CondotelId { get; set; }
        public int HostId { get; set; }
        public int? ResortId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal PricePerNight { get; set; }
        public int Beds { get; set; }
        public int Bathrooms { get; set; }
        public string Status { get; set; }

        // Liên kết 1-n
        public List<ImageDTO>? Images { get; set; }
        public List<PriceDTO>? Prices { get; set; }
        public List<DetailDTO>? Details { get; set; }

        // Liên kết n-n
        public List<AmenityDTO> Amenities { get; set; }
        public List<UtilityDTO> Utilities { get; set; }
        public List<PromotionDTO> Promotions { get; set; }
    }

	public class AmenityDTO
	{
		public int AmenityId { get; set; }
		public string Name { get; set; }
	}

	public class UtilityDTO
	{
		public int UtilityId { get; set; }
		public string Name { get; set; }
	}
}
