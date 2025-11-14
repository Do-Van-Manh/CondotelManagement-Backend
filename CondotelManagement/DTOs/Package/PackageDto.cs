namespace CondotelManagement.DTOs.Package
{
    // DTO nay dung de hien thi thong tin Goc cua cac goi (tren trang Bang gia)
    public class PackageDto
    {
        public int PackageId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; } // SỬA: Giữ là decimal (DTO không nên nullable)
        public string Duration { get; set; } // SỬA: Dùng string Duration (khớp với Model)
        public string Description { get; set; }

        // Quyen loi
        public int MaxListings { get; set; }
        public bool CanUseFeaturedListing { get; set; }
    }
}