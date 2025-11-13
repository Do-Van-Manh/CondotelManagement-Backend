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

		// GET api/tenant/condotels?name=abc&location=abc?...
		[HttpGet]
		public ActionResult<IEnumerable<CondotelDTO>> GetCondotelsByNameAndLocation([FromQuery] string? name, [FromQuery] string? location, [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
		{
			var condotels = _condotelService.GetCondotelsByNameLocationAndDate(name, location, fromDate, toDate);
			return Ok(condotels);
		}
	}
}
