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

        // HÀM LẤY USER ID AN TOÀN – KHÔNG BAO GIỜ BỊ NULL
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
                var userId = GetCurrentUserId(); // Dùng hàm này → không còn NullReference

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
            
            // Lấy danh sách sender IDs để query User một lần
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
                    // Dòng này sẽ thêm chữ 'Z' vào cuối (VD: 05:40Z)
                    // Trình duyệt thấy chữ Z sẽ tự hiểu là UTC và cộng 7 tiếng thành 12:40
                    SentAt = DateTime.SpecifyKind(m.SentAt, DateTimeKind.Utc)
                };
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