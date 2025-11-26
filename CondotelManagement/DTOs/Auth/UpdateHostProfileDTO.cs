namespace CondotelManagement.DTOs
{
	public class UpdateHostProfileDTO
	{
		// HOST
		public string CompanyName { get; set; }
		public string Address { get; set; }
		public string PhoneContact { get; set; }

		// USER
		public string FullName { get; set; }
		public string Phone { get; set; }
		public string Gender { get; set; }
		public DateOnly? DateOfBirth { get; set; }
		public string UserAddress { get; set; }
		public string ImageUrl { get; set; }

		//Wallet
		public string BankName { get; set; }
		public string AccountNumber { get; set; }
		public string AccountHolderName { get; set; }
	}
}
