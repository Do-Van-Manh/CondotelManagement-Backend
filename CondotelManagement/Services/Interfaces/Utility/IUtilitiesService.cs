using CondotelManagement.DTOs;
using CondotelManagement.Models;

namespace CondotelManagement.Services
{
	public interface IUtilitiesService
	{
		Task<IEnumerable<UtilityResponseDTO>> GetUtilitiesByHostAsync(int hostId);
		Task<UtilityResponseDTO> GetByIdAsync(int id, int hostId);
		Task<UtilityResponseDTO> CreateAsync(int hostId, UtilityRequestDTO dto);
		Task<bool> UpdateAsync(int id, int hostId, UtilityRequestDTO dto);
		Task<bool> DeleteAsync(int id, int hostId);
	}
}
