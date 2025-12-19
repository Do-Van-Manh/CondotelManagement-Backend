using CondotelManagement.Services.Interfaces.Chat;
using CondotelManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Controllers.Chat
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        private readonly CondotelDbVer1Context _context;
        public ChatController(IChatService chatService, CondotelDbVer1Context context)
        {
            _chatService = chatService;
            _context = context;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst("nameid")
                     ?? User.FindFirst(ClaimTypes.NameIdentifier)
                     ?? User.FindFirst("sub");

            if (claim == null || !int.TryParse(claim.Value, out int userId))
                throw new UnauthorizedAccessException("Không tìm thấy user ID trong token");

            return userId;
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var userId = GetCurrentUserId(); 

                var conversations = await _chatService.GetMyConversationsWithDetailsAsync(userId);

                var result = conversations.Select(c => new
                {
                    conversationId = c.ConversationId,
                    userAId = c.UserAId,
                    userBId = c.UserBId,
                    // Thông tin user đối phương
                    otherUser = c.OtherUserId.HasValue ? new
                    {
                        userId = c.OtherUserId.Value,
                        fullName = c.OtherUserName,
                        imageUrl = c.OtherUserImageUrl
                    } : null,
                    lastMessage = c.LastMessage != null ? new
                    {
                        content = c.LastMessage.Content,
                        sentAt = c.LastMessage.SentAt,
                        senderId = c.LastMessage.SenderId
                    } : null,
                    unreadCount = c.UnreadCount
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Lỗi server", message = ex.Message });
            }
        }

        [HttpGet("messages/{conversationId}")]
        public async Task<IActionResult> GetMessages(int conversationId, [FromQuery] int take = 100)
        {
            var userId = GetCurrentUserId();
            var msgs = await _chatService.GetMessagesAsync(conversationId, take);
            

            var senderIds = msgs.Select(m => m.SenderId).Distinct().ToList();
            var users = await _context.Users
                .Where(u => senderIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => new { u.FullName, u.ImageUrl });

            var result = msgs.Select(m =>
            {
                var sender = users.ContainsKey(m.SenderId) ? users[m.SenderId] : null;
                return new
                {
                    m.MessageId,
                    m.ConversationId,
                    m.SenderId,
                    sender = sender != null ? new
                    {
                        userId = m.SenderId,
                        fullName = sender.FullName,
                        imageUrl = sender.ImageUrl
                    } : null,
                    m.Content,
                    SentAt = DateTime.SpecifyKind(m.SentAt, DateTimeKind.Utc)
                };
            });

            return Ok(result);
        }

        
        [HttpPost("messages/send-to-host")]
        public async Task<IActionResult> SendToCondotelHost([FromBody] SendMessageToCondotelHostRequest request)
        {
            var senderId = GetCurrentUserId();

            var condotel = await _context.Condotels
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CondotelId == request.CondotelId);

            if (condotel == null)
                return NotFound("Condotel không tồn tại");

            var hostId = condotel.HostId;

            if (hostId == senderId)
                return BadRequest("Không thể chat với chính mình");

            await _chatService.SendDirectMessageAsync(
                senderId,
                hostId,
                request.Content
            );

            return Ok();
        }
        public class SendMessageToCondotelHostRequest
        {
            public int CondotelId { get; set; }
            public string Content { get; set; } = string.Empty;
        }
    }
}