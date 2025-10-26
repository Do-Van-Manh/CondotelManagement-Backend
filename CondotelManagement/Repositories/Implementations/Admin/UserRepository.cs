using CondotelManagement.Data;
using CondotelManagement.Models;
using CondotelManagement.Repositories.Implementations; // 👈 Kế thừa từ file Repository.cs
using CondotelManagement.Repositories.Interfaces;
using CondotelManagement.Repositories.Interfaces.Admin;

namespace CondotelManagement.Repositories.Implementations.Admin
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(CondotelDbVer1Context context) : base(context)
        {
            // Code này "trống" là đúng, vì nó đã kế thừa hết code
            // từ file Repository.cs ở trên.
        }
    }
}