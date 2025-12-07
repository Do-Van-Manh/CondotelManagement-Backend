using CondotelManagement.DTOs.Booking;
using CondotelManagement.Models;
using System;

namespace CondotelManagement.DTOs
{
    public class BookingDTO
    {
        public int BookingId { get; set; }
        public int CondotelId { get; set; }
        public string CondotelName { get; set; }  
        public int CustomerId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal? TotalPrice { get; set; }
        public string Status { get; set; }
        public int? PromotionId { get; set; }
        public string? VoucherCode { get; set; } // Mã voucher để áp dụng
        public int? VoucherId { get; set; } // ID voucher đã áp dụng
        public List<ServicePackageSelectionDTO>? ServicePackages { get; set; } // Danh sách service packages được chọn
        public DateTime CreatedAt { get; set; }
        public bool CanReview { get; set; }
        public bool HasReviewed { get; set; }
    }
}
