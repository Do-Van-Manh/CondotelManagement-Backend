using CondotelManagement.Models;

namespace CondotelManagement.Repositories
{
	public interface IVoucherRepository
	{
		Task<IEnumerable<Voucher>> GetByHostAsync(int hostId);
		Task<IEnumerable<Voucher>> GetByCondotelAsync(int condotelId);
		Task<Voucher?> GetByIdAsync(int id);
		Task<Voucher> AddAsync(Voucher voucher);
		Task<Voucher?> UpdateAsync(Voucher voucher);
		Task<bool> DeleteAsync(int id);
	}
}
