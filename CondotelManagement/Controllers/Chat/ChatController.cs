using CondotelManagement.Services.Interfaces.Chat;
using Microsoft.AspNetCore.Mvc;

namespace CondotelManagement.Controllers.Chat
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;
        public ChatController(IChatService chatService) { _chatService = chatService; }

        [HttpGet("conversations/{userId}")]
        public async Task<IActionResult> GetConversations(int userId)
        {
            var convs = await _chatService.GetMyConversationsAsync(userId);
            return Ok(convs.Select(c => new {
                c.ConversationId,
                c.Name,
                c.ConversationType,
                c.CreatedAt,
                lastMessage = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault()
            }));
        }

        [HttpGet("messages/{conversationId}")]
        public async Task<IActionResult> GetMessages(int conversationId, int take = 100)
        {
            var msgs = await _chatService.GetMessagesAsync(conversationId, take);
            return Ok(msgs);
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
            public string Content { get; set; }
        }

    }

}
