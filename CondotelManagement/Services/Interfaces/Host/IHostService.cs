using HostModel = CondotelManagement.Models.Host;
namespace CondotelManagement.Services
{
    public interface IHostService
    {
        HostModel GetByUserId(int userId);
    }
}
