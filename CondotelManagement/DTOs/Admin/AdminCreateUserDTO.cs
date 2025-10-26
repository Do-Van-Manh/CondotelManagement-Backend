namespace CondotelManagement.DTOs.Admin
{
    public class AdminCreateUserDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } // Sẽ được hash ở service
        public string? Phone { get; set; }
        public int RoleId { get; set; } // Admin phải gán RoleId
        public string? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
    }
}
