namespace CondotelManagement.DTOs
{
    public class UpdateServicePackageDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; }
    }
}
