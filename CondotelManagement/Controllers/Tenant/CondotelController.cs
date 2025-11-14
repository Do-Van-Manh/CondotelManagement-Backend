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

		// GET api/tenant/condotel - Lấy tất cả condotel (không cần đăng nhập)
		[HttpGet]
		[AllowAnonymous]
		public ActionResult<IEnumerable<CondotelDTO>> GetAllCondotel()
		{
			var condotels = _condotelService.GetCondotels();
			return Ok(condotels);
		}

		// GET api/tenant/condotel/{id} - Lấy chi tiết condotel (không cần đăng nhập)
		[HttpGet("{id}")]
		[AllowAnonymous]
		public ActionResult<CondotelDetailDTO> GetCondotelById(int id)
		{
			var condotel = _condotelService.GetCondotelById(id);
			if (condotel == null)
				return NotFound(new { message = "Condotel not found" });

			return Ok(condotel);
		}

		// GET api/tenant/condotel/location?name=Da Nang
		[HttpGet("location")]
		[AllowAnonymous]
		public ActionResult<IEnumerable<CondotelDTO>> GetAllCondotelByLocation([FromQuery] string? name)
		{
			var condotels = _condotelService.GetCondtelsByLocation(name);
			return Ok(condotels);
		}
	}
}
