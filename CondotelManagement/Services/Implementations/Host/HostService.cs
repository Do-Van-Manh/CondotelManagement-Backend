using CondotelManagement.DTOs;
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

		public async Task<HostProfileDTO> GetHostProfileAsync(int userId)
		{
			var host = await _hostRepo.GetHostProfileAsync(userId);
			if (host == null) return null;

			return new HostProfileDTO
			{
				HostID = host.HostId,
				CompanyName = host.CompanyName,
				Address = host.Address,
				PhoneContact = host.PhoneContact,

				UserID = host.User.UserId,
				FullName = host.User.FullName,
				Email = host.User.Email,
				Phone = host.User.Phone,
				Gender = host.User.Gender,
				DateOfBirth = host.User.DateOfBirth,
				UserAddress = host.User.Address,
				ImageUrl = host.User.ImageUrl,

				Packages = host.HostPackages.Select(hp => new HostPackageDTO
				{
					PackageID = hp.PackageId,
					Name = hp.Package.Name,
					StartDate = hp.StartDate,
					EndDate = hp.EndDate
				}).ToList()
			};
		}
		public async Task<bool> UpdateHostProfileAsync(int userId, UpdateHostProfileDTO dto)
		{
			var host = await _hostRepo.GetHostProfileAsync(userId);
			if (host == null) return false;

			// Update HOST
			host.CompanyName = dto.CompanyName;
			host.Address = dto.Address;
			host.PhoneContact = dto.PhoneContact;

			// Update USER
			host.User.FullName = dto.FullName;
			host.User.Phone = dto.Phone;
			host.User.Gender = dto.Gender;
			host.User.DateOfBirth = dto.DateOfBirth;
			host.User.Address = dto.UserAddress;
			host.User.ImageUrl = dto.ImageUrl;

			await _hostRepo.UpdateHostAsync(host);
			return true;
		}
	}
}
