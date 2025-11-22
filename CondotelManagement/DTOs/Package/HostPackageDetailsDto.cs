using System;

namespace CondotelManagement.DTOs.Package
{
    public class HostPackageDetailsDto
    {
        public string PackageName { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string StartDate { get; set; } = null!;  // FE nhận string
        public string EndDate { get; set; } = null!;    // FE nhận string
        public int MaxListings { get; set; }
        public int CurrentListings { get; set; }
        public bool CanUseFeaturedListing { get; set; }

        // THÊM 4 FIELD MỚI CHO PAYOS
        public string? Message { get; set; }
        public string? PaymentUrl { get; set; }
        public long OrderCode { get; set; }      // PayOS yêu cầu số duy nhất
        public decimal Amount { get; set; }      // Số tiền thanh toán
    }
}