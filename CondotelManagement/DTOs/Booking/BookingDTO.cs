using System;

namespace CondotelManagement.DTOs
{
    public class BookingDTO
    {
        public int BookingId { get; set; }
        public int CondotelId { get; set; }
        public int CustomerId { get; set; }
        public DateOnly CheckInDate { get; set; }
        public DateOnly CheckOutDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public string Status { get; set; }
    }
}
