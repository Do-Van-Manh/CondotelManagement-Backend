using CondotelManagement.Repositories;
using HostModel = CondotelManagement.Models.Host;
namespace CondotelManagement.Services
{
    public class HostService : IHostService
    {
        private readonly IHostRepository _hostRepo;

        public HostService(IHostRepository hostRepo)
        {
            _hostRepo = hostRepo;
        }
        public HostModel GetByUserId(int userId)
        {
            return _hostRepo.GetByUserId(userId);
        }
    }
}
