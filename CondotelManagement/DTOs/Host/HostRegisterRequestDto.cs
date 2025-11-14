using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; // 👈 Cần thêm thư viện này

namespace CondotelManagement.DTOs.Host
{
    // DTO này chứa các thông tin BỔ SUNG mà bảng [Host] cần
    public class HostRegisterRequestDto
    {
        // 1. PhoneContact (camelCase ở FE -> PascalCase ở BE)
        [JsonPropertyName("phoneContact")]
        [Required(ErrorMessage = "Số điện thoại liên hệ là bắt buộc.")]
        public string PhoneContact { get; set; } = null!;

        // 2. Address (Không bắt buộc, nhưng vẫn cần ánh xạ tên)
        [JsonPropertyName("address")]
        public string? Address { get; set; }

        // 3. CompanyName (Không bắt buộc, nhưng vẫn cần ánh xạ tên)
        [JsonPropertyName("companyName")]
        public string? CompanyName { get; set; }

        // 4. BankName (Bắt buộc)
        [JsonPropertyName("bankName")]
        [Required(ErrorMessage = "Tên ngân hàng là bắt buộc.")]
        public string BankName { get; set; } = null!;

        // 5. AccountNumber (Bắt buộc)
        [JsonPropertyName("accountNumber")]
        [Required(ErrorMessage = "Số tài khoản là bắt buộc.")]
        public string AccountNumber { get; set; } = null!;

        // 6. AccountHolderName (Bắt buộc)
        [JsonPropertyName("accountHolderName")]
        [Required(ErrorMessage = "Tên chủ tài khoản là bắt buộc.")]
        public string AccountHolderName { get; set; } = null!;
    }
}