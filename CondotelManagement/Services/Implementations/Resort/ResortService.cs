using CondotelManagement.DTOs;
using CondotelManagement.Models;
using CondotelManagement.Repositories;

namespace CondotelManagement.Services
{
    public class ResortService : IResortService
    {
        private readonly IResortRepository _resortRepo;

        public ResortService(IResortRepository resortRepo)
        {
            _resortRepo = resortRepo;
        }

        public async Task<IEnumerable<ResortDTO>> GetAllAsync()
        {
            var resorts = await _resortRepo.GetAllAsync();
            return resorts.Select(r => new ResortDTO
            {
                ResortId = r.ResortId,
                LocationId = r.LocationId,
                Name = r.Name,
                Description = r.Description,
                Location = r.Location != null ? new LocationDTO
                {
                    LocationId = r.Location.LocationId,
                    Name = r.Location.Name,
                    Description = r.Location.Description
                } : null
            });
        }

        public async Task<ResortDTO?> GetByIdAsync(int id)
        {
            var resort = await _resortRepo.GetByIdAsync(id);
            if (resort == null) return null;

            return new ResortDTO
            {
                ResortId = resort.ResortId,
                LocationId = resort.LocationId,
                Name = resort.Name,
                Description = resort.Description,
                Location = resort.Location != null ? new LocationDTO
                {
                    LocationId = resort.Location.LocationId,
                    Name = resort.Location.Name,
                    Description = resort.Location.Description
                } : null
            };
        }

        public async Task<IEnumerable<ResortDTO>> GetByLocationIdAsync(int locationId)
        {
            var resorts = await _resortRepo.GetByLocationIdAsync(locationId);
            return resorts.Select(r => new ResortDTO
            {
                ResortId = r.ResortId,
                LocationId = r.LocationId,
                Name = r.Name,
                Description = r.Description,
                Location = r.Location != null ? new LocationDTO
                {
                    LocationId = r.Location.LocationId,
                    Name = r.Location.Name,
                    Description = r.Location.Description
                } : null
            });
        }

        public async Task<ResortDTO> CreateAsync(ResortCreateUpdateDTO dto)
        {
            var resort = new Resort
            {
                LocationId = dto.LocationId,
                Name = dto.Name,
                Description = dto.Description
            };
            var created = await _resortRepo.AddAsync(resort);
            
            // Reload với Location để có đầy đủ thông tin
            var resortWithLocation = await _resortRepo.GetByIdAsync(created.ResortId);
            
            return new ResortDTO
            {
                ResortId = resortWithLocation!.ResortId,
                LocationId = resortWithLocation.LocationId,
                Name = resortWithLocation.Name,
                Description = resortWithLocation.Description,
                Location = resortWithLocation.Location != null ? new LocationDTO
                {
                    LocationId = resortWithLocation.Location.LocationId,
                    Name = resortWithLocation.Location.Name,
                    Description = resortWithLocation.Location.Description
                } : null
            };
        }

        public async Task<bool> UpdateAsync(int id, ResortCreateUpdateDTO dto)
        {
            var resort = await _resortRepo.GetByIdAsync(id);
            if (resort == null) return false;

            resort.LocationId = dto.LocationId;
            resort.Name = dto.Name;
            resort.Description = dto.Description;

            await _resortRepo.UpdateAsync(resort);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var resort = await _resortRepo.GetByIdAsync(id);
            if (resort == null) return false;

            await _resortRepo.DeleteAsync(resort);
            return true;
        }
    }
}










