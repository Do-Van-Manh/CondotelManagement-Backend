using CondotelManagement.Data;
using CondotelManagement.Models;
using CondotelManagement.Repositories.Implementations; // 👈 Kế thừa từ file Repository.cs
using CondotelManagement.Repositories.Interfaces;
using CondotelManagement.Repositories.Interfaces.Admin;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Repositories.Implementations.Admin
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        // Tên DbContext lấy từ thông báo lỗi của bạn
        private readonly CondotelDbVer1Context _context;

        // SỬA LỖI CS7036: Thêm ": base(context)"
        public UserRepository(CondotelDbVer1Context context) : base(context)
        {
            _context = context;
        }

        // Phương thức này bây giờ sẽ hoạt động
        public async Task<User?> GetByIdAsync(int userId)
        {
            // .Include() sẽ được tìm thấy do có using ở trên
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}