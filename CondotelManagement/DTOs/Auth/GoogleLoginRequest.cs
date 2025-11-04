using System.ComponentModel.DataAnnotations;

namespace CondotelManagement.DTOs.Auth
{
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = null!;
    }
}