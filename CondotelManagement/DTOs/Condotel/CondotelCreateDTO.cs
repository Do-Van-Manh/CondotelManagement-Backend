using System.Text.Json.Serialization;

namespace CondotelManagement.DTOs
{
    public class CondotelCreateDTO
    {
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

    public class ImageDTO
    {
        public int ImageId { get; set; }
        public string ImageUrl { get; set; }
        public string? Caption { get; set; }
    }

    public class PriceDTO
    {
        public int PriceId { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal BasePrice { get; set; }
        public string PriceType { get; set; }

        public string Description { get; set; }
    }

    public class DetailDTO
    {
        public string? BuildingName { get; set; }
        public string? RoomNumber { get; set; }
        public byte Beds { get; set; }
        public byte Bathrooms { get; set; }
        public string? SafetyFeatures { get; set; }
        public string? HygieneStandards { get; set; }
    }
}
