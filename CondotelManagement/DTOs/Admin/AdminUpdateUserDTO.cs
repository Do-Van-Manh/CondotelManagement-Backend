namespace CondotelManagement.DTOs.Admin
{
    public class AdminUpdateUserDTO
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public int RoleId { get; set; }
        public string? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Address { get; set; }
    }
}
