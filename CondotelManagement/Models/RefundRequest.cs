using System;

namespace CondotelManagement.Models
{
    public partial class RefundRequest
    {
        public int Id { get; set; }
        public int BookingId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string? CustomerEmail { get; set; }
        
        // Thông tin hoàn tiền
        public decimal RefundAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Completed, Refunded, Rejected
        
        // Thông tin ngân hàng
        public string? BankCode { get; set; }
        public string? AccountNumber { get; set; }
        public string? AccountHolder { get; set; }
        
        // Thông tin xử lý
        public string? Reason { get; set; }
        public DateTime? CancelDate { get; set; }
        public int? ProcessedBy { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string? TransactionId { get; set; }
        public string? PaymentMethod { get; set; } // 'Auto' (PayOS) hoặc 'Manual'
        
        // Timestamps
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual Booking Booking { get; set; } = null!;
        public virtual User Customer { get; set; } = null!;
        public virtual User? ProcessedByUser { get; set; }
    }
}


