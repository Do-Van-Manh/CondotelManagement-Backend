namespace CondotelManagement.DTOs.Tenant
{
    public class ReviewQueryDTO
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? CondotelId { get; set; }
        public byte? MinRating { get; set; }
        public byte? MaxRating { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}
