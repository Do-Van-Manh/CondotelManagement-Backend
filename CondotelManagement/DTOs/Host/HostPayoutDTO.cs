namespace CondotelManagement.DTOs.Host
{
    public class HostPayoutResponseDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int ProcessedCount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<HostPayoutItemDTO> ProcessedItems { get; set; } = new List<HostPayoutItemDTO>();
    }

    public class HostPayoutItemDTO
    {
        public int BookingId { get; set; }
        public int CondotelId { get; set; }
        public string CondotelName { get; set; } = string.Empty;
        public int HostId { get; set; }
        public string HostName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateOnly EndDate { get; set; }
        public DateTime? PaidAt { get; set; }
        public bool IsPaid { get; set; }
        public int DaysSinceCompleted { get; set; }
    }
}

