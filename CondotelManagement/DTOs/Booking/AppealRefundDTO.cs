using System.ComponentModel.DataAnnotations;

namespace CondotelManagement.DTOs.Booking
{
    public class AppealRefundDTO
    {
        [Required(ErrorMessage = "Lý do kháng cáo không được bỏ trống")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Lý do kháng cáo phải từ 10 đến 500 ký tự")]
        public string AppealReason { get; set; } = null!;
    }
}
