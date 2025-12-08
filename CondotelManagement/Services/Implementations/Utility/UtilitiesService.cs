using CondotelManagement.DTOs;
using CondotelManagement.Models;
using CondotelManagement.Repositories;
using Google.Apis.Util;

namespace CondotelManagement.Services 
{
	public class UtilitiesService : IUtilitiesService
	{
		private readonly IUtilitiesRepository _repository;

		public UtilitiesService(IUtilitiesRepository repo)
		{
			_repository = repo;
		}

		// GET ALL UTILITIES OF HOST
		public async Task<IEnumerable<UtilityResponseDTO>> GetUtilitiesByHostAsync(int hostId)
		{
			var utilities = await _repository.GetByHostAsync(hostId);

			return utilities.Select(u => new UtilityResponseDTO
			{
				UtilityId = u.UtilityId,
				HostId = u.HostId,
				Name = u.Name,
				Category = u.Category,
				Description = u.Description
			});
		}

		// GET BY ID (host scope)
		public async Task<UtilityResponseDTO?> GetByIdAsync(int id, int hostId)
		{
			var utility = await _repository.GetByIdAsync(id);

			if (utility == null || utility.HostId != hostId)
				return null;

			return new UtilityResponseDTO
			{
				UtilityId = utility.UtilityId,
				HostId = utility.HostId,
				Name = utility.Name,
				Category = utility.Category,
				Description = utility.Description
			};
		}

		// CREATE NEW UTILITY
		public async Task<UtilityResponseDTO> CreateAsync(int hostId, UtilityRequestDTO dto)
		{
			var utility = new Utility
			{
				HostId = hostId,
				Name = dto.Name,
				Description = dto.Description,
				Category = dto.Category
			};

			var created = await _repository.CreateAsync(utility);

			return new UtilityResponseDTO
			{
				UtilityId = created.UtilityId,
				HostId = created.HostId,
				Name = created.Name,
				Description = created.Description,
				Category = created.Category
			};
		}

		// UPDATE (only own utilities)
		public async Task<bool> UpdateAsync(int id, int hostId, UtilityRequestDTO dto)
		{
			var entity = await _repository.GetByIdAsync(id);

			if (entity == null || entity.HostId != hostId)
				return false;

			entity.Name = dto.Name;
			entity.Description = dto.Description;
			entity.Category = dto.Category;

			return await _repository.UpdateAsync(entity);
		}

		// DELETE (host only)
		public async Task<bool> DeleteAsync(int id, int hostId)
		{
			return await _repository.DeleteAsync(id, hostId);
		}

		// ========== ADMIN METHODS ==========

		// GET ALL UTILITIES (Admin)
		public async Task<IEnumerable<UtilityResponseDTO>> AdminGetAllAsync()
		{
			var utilities = await _repository.GetAllAsync();

			return utilities.Select(u => new UtilityResponseDTO
			{
				UtilityId = u.UtilityId,
				HostId = u.HostId,
				Name = u.Name,
				Category = u.Category,
				Description = u.Description
			});
		}

		// GET BY ID (Admin)
		public async Task<UtilityResponseDTO?> AdminGetByIdAsync(int id)
		{
			var utility = await _repository.GetByIdAsync(id);
			if (utility == null) return null;

			return new UtilityResponseDTO
			{
				UtilityId = utility.UtilityId,
				HostId = utility.HostId,
				Name = utility.Name,
				Category = utility.Category,
				Description = utility.Description
			};
		}

		// CREATE (Admin) - Admin tạo utility với HostId = 0 (system utility)
		public async Task<UtilityResponseDTO> AdminCreateAsync(UtilityRequestDTO dto)
		{
			var utility = new Utility
			{
				HostId = 0, // 0 = System/Admin utility
				Name = dto.Name,
				Description = dto.Description,
				Category = dto.Category
			};

			var created = await _repository.CreateAsync(utility);

			return new UtilityResponseDTO
			{
				UtilityId = created.UtilityId,
				HostId = created.HostId,
				Name = created.Name,
				Description = created.Description,
				Category = created.Category
			};
		}

		// UPDATE (Admin) - Admin có thể update bất kỳ utility nào
		public async Task<bool> AdminUpdateAsync(int id, UtilityRequestDTO dto)
		{
			var entity = await _repository.GetByIdAsync(id);
			if (entity == null) return false;

			entity.Name = dto.Name;
			entity.Description = dto.Description;
			entity.Category = dto.Category;

			return await _repository.UpdateAsync(entity);
		}

		// DELETE (Admin) - Admin có thể xóa bất kỳ utility nào
		public async Task<bool> AdminDeleteAsync(int id)
		{
			return await _repository.AdminDeleteAsync(id);
		}
	}
}
