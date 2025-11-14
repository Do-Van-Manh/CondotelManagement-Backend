namespace CondotelManagement.Services.Interfaces
{
    // Interface nay dinh nghia cac quyen loi cua tung goi
    public interface IPackageFeatureService
    {
        int GetMaxListingCount(int packageId);
        bool CanUseFeaturedListing(int packageId);
    }
}