using System;

namespace CondotelManagement.DTOs
{
    public class BookingDTO
    {
        public int BookingId { get; set; }
        public int CondotelId { get; set; }
        public int CustomerId { get; set; }
        public DateOnly StartDate { get; set; }   
        public DateOnly EndDate { get; set; }     
        public decimal? TotalPrice { get; set; }  
        public string Status { get; set; }
        public int? PromotionId { get; set; }
        public bool IsUsingRewardPoints { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
