using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers
{
	[ApiController]
	[Route("api/tenant/condotel")]
	public class CondotelController : ControllerBase
	{
		private readonly ICondotelService _condotelService;

		public CondotelController(ICondotelService condotelService)
		{
			_condotelService = condotelService;
		}

		// GET api/tenant/condotel/location?name=Da Nang
		[HttpGet("location")]
		public ActionResult<IEnumerable<CondotelDTO>> GetAllCondotelByLocation([FromQuery] string? name)
		{
			var condotels = _condotelService.GetCondtelsByLocation(name);
			return Ok(condotels);
		}
	}
}
