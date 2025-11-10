using CondotelManagement.DTOs;
using CondotelManagement.Helpers;
using CondotelManagement.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Host
{
	[ApiController]
	[Route("api/host/[controller]")]
	[Authorize(Roles = "Host")]
	public class ReviewController : ControllerBase
	{
		private readonly IReviewService _reviewService;
		private readonly IHostService _hostService;

		public ReviewController(IReviewService reviewService, IHostService hostService)
		{
			_reviewService = reviewService;
			_hostService = hostService;
		}
		// Host xem tất cả reviews condotel của mình
		[HttpGet]
		public async Task<IActionResult> GetByHost()
		{             //current host login
			var hostId = _hostService.GetByUserId(User.GetUserId()).HostId;
			return Ok(await _reviewService.GetReviewsByHostAsync(hostId));
		}

		// Host trả lời review
		[HttpPut("{reviewId}/reply")]
		public async Task<IActionResult> ReplyReview(int reviewId, [FromBody] ReviewReplyDTO dto)
		{
			await _reviewService.ReplyToReviewAsync(reviewId, dto.Reply);
			return Ok(new { message = "Đã trả lời review" });
		}

		// Host report review
		[HttpPut("{reviewId}/report")]
		public async Task<IActionResult> ReportReview(int reviewId)
		{
			await _reviewService.UpdateReviewStatusAsync(reviewId, "Reported");
			return Ok(new { message = "Đã report review" });
		}
	}
}
