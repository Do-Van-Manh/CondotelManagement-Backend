using CondotelManagement.Services.Interfaces.Chat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CondotelManagement.Controllers.Chat
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
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
                    otherUserName = c.OtherUserName,
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
            var result = msgs.Select(m => new
            {
                m.MessageId,
                m.ConversationId,
                m.SenderId,
                senderName = m.Sender.FullName,
                m.Content,
                SentAt = DateTime.SpecifyKind(m.SentAt, DateTimeKind.Utc)
            });

            return Ok(result);
        }

        [HttpPost("messages/send-direct")]
        public async Task<IActionResult> SendDirectMessage([FromBody] DirectMessageRequest request)
        {
            await _chatService.SendDirectMessageAsync(request.SenderId, request.ReceiverId, request.Content);
            return Ok();
        }

        public class DirectMessageRequest
        {
            public int SenderId { get; set; }
            public int ReceiverId { get; set; }
            public string Content { get; set; } = string.Empty;
        }
    }
}