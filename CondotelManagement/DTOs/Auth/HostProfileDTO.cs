namespace CondotelManagement.DTOs
{
	public class HostProfileDTO
	{
		public int HostID { get; set; }
		public string CompanyName { get; set; }
		public string Address { get; set; }
		public string PhoneContact { get; set; }

		// ------ USER INFO ------
		public int UserID { get; set; }
		public string FullName { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Gender { get; set; }
		public DateOnly? DateOfBirth { get; set; }
		public string UserAddress { get; set; }
		public string ImageUrl { get; set; }

		// ------ PACKAGE INFO ------
		public List<HostPackageDTO> Packages { get; set; }
	}

	public class HostPackageDTO
	{
		public int PackageID { get; set; }
		public string Name { get; set; }
		public DateOnly? StartDate { get; set; }
		public DateOnly? EndDate { get; set; }
	}
}
