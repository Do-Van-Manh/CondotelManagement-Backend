namespace CondotelManagement.DTOs
{
    public class CustomerBookingDTO
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }

        public int BookingId { get; set; }
        public string CondotelName { get; set; }
        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }
        public DateTime BookingDate { get; set; }
        public decimal? TotalPrice { get; set; }
        public string Status { get; set; }
    }
}
