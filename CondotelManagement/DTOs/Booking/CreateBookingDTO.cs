using System.ComponentModel.DataAnnotations;

namespace CondotelManagement.DTOs.Booking
{
    public class CreateBookingDTO
    {
        [Required]
        public int CondotelId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        public int? PromotionId { get; set; }

        public string? VoucherCode { get; set; }

        public List<ServicePackageSelectionDTO>? ServicePackages { get; set; }
    }

   
}
