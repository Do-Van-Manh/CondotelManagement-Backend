using System.Text.Json.Serialization;

namespace CondotelManagement.DTOs.Auth
{
    public class LoginRequest
    {
        // Thêm thuộc tính này để ánh xạ JSON camelCase từ FE
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        // Thêm thuộc tính này để ánh xạ JSON camelCase từ FE
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}
