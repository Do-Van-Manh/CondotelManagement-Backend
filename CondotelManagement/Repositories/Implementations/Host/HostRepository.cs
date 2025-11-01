using HostModel = CondotelManagement.Models.Host;
using CondotelManagement.Data;

namespace CondotelManagement.Repositories
{
    public class HostRepository : IHostRepository
    {
        private readonly CondotelDbVer1Context _context;

        public HostRepository(CondotelDbVer1Context context)
        {
            _context = context;
        }

        HostModel IHostRepository.GetByUserId(int userId)
        {
            return _context.Hosts.FirstOrDefault(h => h.UserId == userId);
        }
    }
}
