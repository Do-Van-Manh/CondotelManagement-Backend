
namespace CondotelManagement.Hub
{

    using CondotelManagement.Services.Interfaces.Chat;
    using Microsoft.AspNetCore.SignalR;
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        public ChatHub(IChatService chatService) { _chatService = chatService; }

        
        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conv-{conversationId}");
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"conv-{conversationId}");
        }

   
        public async Task SendMessage(int conversationId, int senderId, string content)
        {
           
            await _chatService.SendMessageAsync(conversationId, senderId, content);

            
            var dto = new
            {
                conversationId,
                senderId,
                content,
                sentAt = DateTime.Now
            };

            await Clients.Group($"conv-{conversationId}").SendAsync("ReceiveMessage", dto);
        }

   
        public async Task<int> GetOrCreateDirectConversation(int meUserId, int otherUserId)
        {
            var conv = await _chatService.GetOrCreateDirectConversationAsync(meUserId, otherUserId);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conv-{conv.ConversationId}");
            return conv.ConversationId;
        }
    }

}
