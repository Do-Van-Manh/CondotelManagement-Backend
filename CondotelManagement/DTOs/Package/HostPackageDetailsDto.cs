using System;

namespace CondotelManagement.DTOs.Package
{
    // DTO nay dung de hien thi goi ma Host dang so huu (API /my-package)
    public class HostPackageDetailsDto
    {
        // XOA: HostPackageId (Model cua ban khong co cot nay)

        public string PackageName { get; set; }
        public string Status { get; set; }
        public DateOnly StartDate { get; set; } // SỬA: Dùng DateOnly (khớp với Model)
        public DateOnly EndDate { get; set; }   // SỬA: Dùng DateOnly (khớp với Model)

        // Cac quyen loi cua goi nay
        public int MaxListings { get; set; }
        public int CurrentListings { get; set; }
        public bool CanUseFeaturedListing { get; set; }
    }
}