using CondotelManagement.DTOs;
using CondotelManagement.Models;
using CondotelManagement.Repositories;

namespace CondotelManagement.Services
{
    public class ServicePackageService : IServicePackageService
    {
        private readonly IServicePackageRepository _repo;

        public ServicePackageService(IServicePackageRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<ServicePackageDTO>> GetAllAsync()
        {
            var data = await _repo.GetAllAsync();
            return data.Select(x => new ServicePackageDTO
            {
                ServiceId = x.ServiceId,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                Status = x.Status
            });
        }

        public async Task<ServicePackageDTO?> GetByIdAsync(int id)
        {
            var x = await _repo.GetByIdAsync(id);
            if (x == null) return null;

            return new ServicePackageDTO
            {
                ServiceId = x.ServiceId,
                Name = x.Name,
                Description = x.Description,
                Price = x.Price,
                Status = x.Status
            };
        }

        public async Task<ServicePackageDTO> CreateAsync(CreateServicePackageDTO dto)
        {
            var entity = new ServicePackage
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Status = "Active"
            };

            await _repo.AddAsync(entity);
            await _repo.SaveAsync();

            return new ServicePackageDTO
            {
                ServiceId = entity.ServiceId,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Status = entity.Status
            };
        }

        public async Task<ServicePackageDTO?> UpdateAsync(int id, UpdateServicePackageDTO dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;

            entity.Name = dto.Name;
            entity.Description = dto.Description;
            entity.Price = dto.Price;
            entity.Status = dto.Status;

            await _repo.UpdateAsync(entity);
            await _repo.SaveAsync();

            return new ServicePackageDTO
            {
                ServiceId = entity.ServiceId,
                Name = entity.Name,
                Description = entity.Description,
                Price = entity.Price,
                Status = entity.Status
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return false;

            // Soft delete
            entity.Status = "Inactive";
            await _repo.UpdateAsync(entity);
            await _repo.SaveAsync();
            return true;
        }
    }
}
