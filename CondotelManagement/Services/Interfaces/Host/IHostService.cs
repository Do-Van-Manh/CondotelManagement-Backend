using CondotelManagement.DTOs;
using HostModel = CondotelManagement.Models.Host;
namespace CondotelManagement.Services
{
    public interface IHostService
    {
        HostModel GetByUserId(int userId);
		Task<HostProfileDTO> GetHostProfileAsync(int userId);
		Task<bool> UpdateHostProfileAsync(int userId, UpdateHostProfileDTO dto);
	}
}
