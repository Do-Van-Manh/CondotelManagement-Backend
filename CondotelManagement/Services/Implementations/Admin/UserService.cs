using CondotelManagement.Data;
using CondotelManagement.DTOs.Admin;
using CondotelManagement.Models;
using CondotelManagement.Services.Interfaces.Admin;
using Microsoft.EntityFrameworkCore; 
using CondotelManagement.Repositories.Interfaces;
using CondotelManagement.Repositories.Interfaces.Admin;

namespace CondotelManagement.Services.Implementations.Admin
{
    public class UserService : IUserService
    {
        // Giả sử bạn dùng Repository Pattern
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly CondotelDbVer1Context _context; // Hoặc inject DbContext

        public UserService(IUserRepository userRepository,
                             IRoleRepository roleRepository,
                             CondotelDbVer1Context context)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _context = context; // Dùng context để Query (Select/Include)
        }

        // 1. Lấy tất cả user
        public async Task<IEnumerable<UserViewDTO>> AdminGetAllUsersAsync()
        {
            // Dùng _context để có thể Include và Select (Project) sang DTO
            return await _context.Users
                .Include(u => u.Role) // Join với bảng Role
                .Select(u => new UserViewDTO // Map sang DTO
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Status = u.Status,
                    Gender = u.Gender,
                    DateOfBirth = u.DateOfBirth,
                    Address = u.Address,
                    CreatedAt = u.CreatedAt,
                    RoleName = u.Role.RoleName // Lấy tên Role
                })
                .ToListAsync();
        }

        // 2. Lấy user theo ID
        public async Task<UserViewDTO> AdminGetUserByIdAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .Where(u => u.UserId == userId)
                .Select(u => new UserViewDTO
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone,
                    Status = u.Status,
                    Gender = u.Gender,
                    DateOfBirth = u.DateOfBirth,
                    Address = u.Address,
                    CreatedAt = u.CreatedAt,
                    RoleName = u.Role.RoleName
                })
                .FirstOrDefaultAsync();
        }

        // 3. Admin tạo user
        public async Task<(bool IsSuccess, string Message, UserViewDTO CreatedUser)> AdminCreateUserAsync(AdminCreateUserDTO dto)
        {
            // 1. Kiểm tra email tồn tại
            var existingUser = await _userRepository.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
            {
                return (false, "Email đã tồn tại", null);
            }

            // 2. Kiểm tra RoleId có hợp lệ
            var roleExists = await _roleRepository.GetByIdAsync(dto.RoleId);
            if (roleExists == null)
            {
                return (false, "RoleId không hợp lệ", null);
            }

            // 3. Hash mật khẩu (Sử dụng BCrypt.Net)
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 4. Tạo Model User mới
            var newUser = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = passwordHash,
                Phone = dto.Phone,
                RoleId = dto.RoleId,
                Gender = dto.Gender,
                DateOfBirth = dto.DateOfBirth,
                Address = dto.Address,
                Status = "Active", // Gán trạng thái mặc định
                CreatedAt = DateTime.UtcNow // Gán thời gian tạo
            };

            // 5. Lưu vào DB
            await _userRepository.AddAsync(newUser);

            // 6. Map sang DTO để trả về
            var userView = new UserViewDTO
            {
                UserId = newUser.UserId,
                FullName = newUser.FullName,
                Email = newUser.Email,
                Phone = newUser.Phone,
                Status = newUser.Status,
                Gender = newUser.Gender,
                DateOfBirth = newUser.DateOfBirth,
                Address = newUser.Address,
                CreatedAt = newUser.CreatedAt,
                RoleName = roleExists.RoleName // Dùng tên Role đã check ở trên
            };

            return (true, "Tạo user thành công", userView);
        }

        // 4. Admin cập nhật user
        public async Task<(bool IsSuccess, string Message, UserViewDTO UpdatedUser)> AdminUpdateUserAsync(int userId, AdminUpdateUserDTO dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return (false, "Không tìm thấy user", null);
            }

            // Kiểm tra email trùng lặp (nếu đổi email)
            if (user.Email != dto.Email)
            {
                var emailExists = await _userRepository.FirstOrDefaultAsync(u => u.Email == dto.Email);
                if (emailExists != null)
                {
                    return (false, "Email mới đã được sử dụng", null);
                }
            }

            // Kiểm tra RoleId
            var role = await _roleRepository.GetByIdAsync(dto.RoleId);
            if (role == null)
            {
                return (false, "RoleId không hợp lệ", null);
            }

            // Cập nhật thông tin
            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Phone = dto.Phone;
            user.RoleId = dto.RoleId;
            user.Gender = dto.Gender;
            user.DateOfBirth = dto.DateOfBirth;
            user.Address = dto.Address;

            await _userRepository.UpdateAsync(user);

            // Map sang DTO trả về
            var updatedView = await AdminGetUserByIdAsync(userId); // Gọi lại hàm GetById để lấy DTO
            return (true, "Cập nhật thành công", updatedView);
        }

        // 5. Admin reset mật khẩu
        public async Task<bool> AdminResetPasswordAsync(int userId, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        // 6. Admin cập nhật trạng thái
        public async Task<bool> AdminUpdateUserStatusAsync(int userId, string newStatus)
        {
            // TODO: Bạn nên kiểm tra xem newStatus có hợp lệ không
            // (ví dụ: chỉ cho phép "Active", "Locked", "Deleted")

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            user.Status = newStatus;
            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}
