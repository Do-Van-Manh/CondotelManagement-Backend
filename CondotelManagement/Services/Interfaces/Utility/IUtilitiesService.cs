using CondotelManagement.DTOs;
using CondotelManagement.Models;

namespace CondotelManagement.Services
{
	public interface IUtilitiesService
	{
		// Host methods
		Task<IEnumerable<UtilityResponseDTO>> GetUtilitiesByHostAsync(int hostId);
		Task<UtilityResponseDTO?> GetByIdAsync(int id, int hostId);
		Task<UtilityResponseDTO> CreateAsync(int hostId, UtilityRequestDTO dto);
		Task<bool> UpdateAsync(int id, int hostId, UtilityRequestDTO dto);
		Task<bool> DeleteAsync(int id, int hostId);

		// Admin methods
		Task<IEnumerable<UtilityResponseDTO>> AdminGetAllAsync();
		Task<UtilityResponseDTO?> AdminGetByIdAsync(int id);
		Task<UtilityResponseDTO> AdminCreateAsync(UtilityRequestDTO dto);
		Task<bool> AdminUpdateAsync(int id, UtilityRequestDTO dto);
		Task<bool> AdminDeleteAsync(int id);
	}
}
