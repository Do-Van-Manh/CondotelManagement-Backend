using CondotelManagement.Services.Interfaces;

namespace CondotelManagement.Services.Implementations
{
    // Day la noi ban "hard-code" cac quyen loi
    public class PackageFeatureService : IPackageFeatureService
    {
        // GIA SU:
        // PackageID 1 = Goi Basic
        // PackageID 2 = Goi Premium
        // PackageID 3 = Goi Pro

        public int GetMaxListingCount(int packageId)
        {
            switch (packageId)
            {
                case 1: return 5;  // Basic: 5 can ho
                case 2: return 20; // Premium: 20 can ho
                case 3: return 100; // Pro: 100 can ho
                default: return 0; // Goi het han hoac khong ro
            }
        }

        public bool CanUseFeaturedListing(int packageId)
        {
            // Chi goi Premium (2) va Pro (3) moi duoc dang tin noi bat
            return packageId == 2 || packageId == 3;
        }
    }
}