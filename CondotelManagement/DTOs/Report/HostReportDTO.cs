namespace CondotelManagement.DTOs
{
    public class HostReportDTO
    {
        public decimal Revenue { get; set; }
        public int TotalRooms { get; set; }
        public int RoomsBooked { get; set; }
        public double OccupancyRate { get; set; } // ví dụ: 45.5 (phần trăm)
        public int TotalBookings { get; set; }
        public int TotalCancellations { get; set; }
    }
}
