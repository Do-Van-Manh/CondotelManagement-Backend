using System.ComponentModel.DataAnnotations;

namespace CondotelManagement.DTOs.Tenant
{
    public class RedeemPointsDTO
    {
        [Required(ErrorMessage = "BookingID is required")]
        public int BookingId { get; set; }

        [Required(ErrorMessage = "Points to redeem is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Points must be greater than 0")]
        public int PointsToRedeem { get; set; }
    }
}
