using CondotelManagement.Data;
using CondotelManagement.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly CondotelDbVer1Context _context;

        public CustomerRepository(CondotelDbVer1Context context)
        {
            _context = context;
        }

        public async Task<List<CustomerBookingDTO>> GetCustomersByHostIdAsync(int hostId)
        {
            return await _context.Bookings
            .Where(b => b.Condotel.HostId == hostId)
            .Select(b => new CustomerBookingDTO
            {
                UserId = b.Customer.UserId,
                FullName = b.Customer.FullName,
                Email = b.Customer.Email,
                Phone = b.Customer.Phone,
                Gender = b.Customer.Gender,
                DateOfBirth = b.Customer.DateOfBirth,
                Address = b.Customer.Address
            })
            .OrderByDescending(x => x.UserId)
            .ToListAsync();
        }
    }
}
