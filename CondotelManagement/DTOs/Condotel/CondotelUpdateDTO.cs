using System.Text.Json.Serialization;

namespace CondotelManagement.DTOs
{
    public class CondotelUpdateDTO
    {
        public int CondotelId { get; set; }
        [JsonIgnore] // Không cho client set
        public int HostId { get; set; }
        public int? ResortId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal PricePerNight { get; set; }
        public int Beds { get; set; }
        public int Bathrooms { get; set; }
        public string Status { get; set; }

        // Liên kết 1-n
        public List<ImageDTO>? Images { get; set; }
        public List<PriceDTO>? Prices { get; set; }
        public List<DetailDTO>? Details { get; set; }

        // Liên kết n-n
        public List<int>? AmenityIds { get; set; }
        public List<int>? UtilityIds { get; set; }
    }
}
