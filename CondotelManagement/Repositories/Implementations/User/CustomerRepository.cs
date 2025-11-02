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
                BookingId = b.BookingId,
                CondotelName = b.Condotel.Name,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                BookingDate = b.CreatedAt,
                TotalPrice = b.TotalPrice,
                Status = b.Status
            })
            .OrderByDescending(x => x.BookingDate)
            .ToListAsync();
        }
    }
}
