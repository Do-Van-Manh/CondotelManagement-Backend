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
    }
}

