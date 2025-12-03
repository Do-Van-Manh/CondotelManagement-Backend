using CondotelManagement.Services.Interfaces;

namespace CondotelManagement.Services.Implementations
{
    public class PackageFeatureService : IPackageFeatureService
    {
        // PackageID 1 = Gói Cơ Bản (Basic)
        // PackageID 2 = Gói Cao Cấp (Premium)

        public int GetMaxListingCount(int packageId)
        {
            switch (packageId)
            {
                case 1: return 3;   // Cơ Bản: 3 condotel
                case 2: return 10;  // Cao Cấp: 10 condotel
                default: return 0;  // Chưa có gói hoặc hết hạn
            }
        }

        public bool CanUseFeaturedListing(int packageId)
        {
            // Chỉ gói Cao Cấp (2) mới được đăng tin nổi bật
            return packageId == 2;
        }

        public int GetMaxBlogRequestsPerMonth(int packageId)
        {
            // Chỉ gói Cao Cấp được yêu cầu đăng blog, tối đa 5 blog/tháng
            return packageId == 2 ? 5 : 0;
        }

        public bool IsVerifiedBadgeEnabled(int packageId)
        {
            // Chỉ gói Cao Cấp có badge "Đã xác minh"
            return packageId == 2;
        }

        public string GetDisplayColorTheme(int packageId)
        {
            switch (packageId)
            {
                case 1: return "default";     // Màu mặc định cho Cơ Bản
                case 2: return "premium-gold"; // Màu vàng/gold cho Cao Cấp
                default: return "default";
            }
        }

        public int GetPriorityLevel(int packageId)
        {
            // Mức độ ưu tiên hiển thị (số càng cao càng ưu tiên)
            switch (packageId)
            {
                case 2: return 10; // Cao Cấp: ưu tiên cao nhất
                case 1: return 5;  // Cơ Bản: ưu tiên thấp
                default: return 0;
            }
        }
    }
}