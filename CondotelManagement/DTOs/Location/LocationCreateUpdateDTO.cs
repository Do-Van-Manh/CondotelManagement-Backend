using System.ComponentModel.DataAnnotations;

namespace CondotelManagement.DTOs
{
    public class LocationCreateUpdateDTO
    {
		[Required(ErrorMessage = "Tên địa điểm không được bỏ trống.")]
		[MaxLength(150, ErrorMessage = "Tên địa điểm không được vượt quá 150 ký tự.")]
		public string Name { get; set; } = null!;

		[MaxLength(255, ErrorMessage = "Mô tả không được vượt quá 255 ký tự.")]
		public string? Description { get; set; }
	}
}
