namespace CondotelManagement.Services.Interfaces
{
    public interface IPackageFeatureService
    {
        // Số lượng condotel tối đa
        int GetMaxListingCount(int packageId);

        // Có được đăng tin nổi bật không
        bool CanUseFeaturedListing(int packageId);

        // Số lượng blog request tối đa mỗi tháng
        int GetMaxBlogRequestsPerMonth(int packageId);

        // Có hiển thị badge "Đã xác minh" không
        bool IsVerifiedBadgeEnabled(int packageId);

        // Theme màu hiển thị (cho FE)
        string GetDisplayColorTheme(int packageId);

        // Mức độ ưu tiên hiển thị
        int GetPriorityLevel(int packageId);
    }
}