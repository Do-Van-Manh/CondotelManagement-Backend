using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers
{
	[ApiController]
	[Route("api/tenant/condotels")]
	public class CondotelController : ControllerBase
	{
		private readonly ICondotelService _condotelService;

		public CondotelController(ICondotelService condotelService)
		{
			_condotelService = condotelService;
		}

		// GET api/tenant/condotels?name=abc&location=abc&fromDate=...&toDate=...
		[HttpGet]
		[AllowAnonymous]
		public ActionResult<IEnumerable<CondotelDTO>> GetCondotelsByFilters(
				[FromQuery] string? name, 
				[FromQuery] string? location, 
				[FromQuery] DateOnly? fromDate, 
				[FromQuery] DateOnly? toDate,
				[FromQuery] decimal? minPrice,
				[FromQuery] decimal? maxPrice,
				[FromQuery] int? beds,
				[FromQuery] int? bathrooms)
		{
			var condotels = _condotelService.GetCondotelsByFilters(name, location, fromDate, toDate, minPrice, maxPrice, beds, bathrooms);
			return Ok(condotels);
		}

		// GET api/tenant/condotels/{id} - Lấy chi tiết condotel (không cần đăng nhập)
		[HttpGet("{id}")]
		[AllowAnonymous]
		public ActionResult<CondotelDetailDTO> GetCondotelById(int id)
		{
			var condotel = _condotelService.GetCondotelById(id);
			if (condotel == null)
				return NotFound(new { message = "Condotel not found" });

			return Ok(condotel);
		}
	}
}
