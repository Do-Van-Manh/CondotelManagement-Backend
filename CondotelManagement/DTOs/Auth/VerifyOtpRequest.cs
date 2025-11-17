using System.ComponentModel.DataAnnotations;

namespace CondotelManagement.DTOs.Auth
{
    public class VerifyOtpRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [Length(6, 6)]
        public string Otp { get; set; } = null!;
    }
}

