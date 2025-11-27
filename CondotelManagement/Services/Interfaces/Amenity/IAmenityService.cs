using CondotelManagement.DTOs.Amenity;

namespace CondotelManagement.Services.Interfaces.Amenity
{
    public interface IAmenityService
    {
        Task<IEnumerable<AmenityResponseDTO>> GetAllAsync();
        Task<AmenityResponseDTO?> GetByIdAsync(int id);
        Task<AmenityResponseDTO> CreateAsync(AmenityRequestDTO dto);
        Task<bool> UpdateAsync(int id, AmenityRequestDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}

