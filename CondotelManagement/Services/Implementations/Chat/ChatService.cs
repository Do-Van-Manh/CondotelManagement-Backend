using CondotelManagement.Models;
using CondotelManagement.Repositories.Interfaces.Chat;
using CondotelManagement.Services.Interfaces.Chat;

namespace CondotelManagement.Services.Implementations.Chat
{
   
    public class ChatService : IChatService
    {
        private readonly IChatRepository _repo;
        public ChatService(IChatRepository repo) { _repo = repo; }

        public async Task<ChatConversation> GetOrCreateDirectConversationAsync(int meUserId, int otherUserId)
        {
            var conv = await _repo.GetDirectConversationAsync(meUserId, otherUserId);
            if (conv != null) return conv;

            var newConv = new ChatConversation
            {
                ConversationType = "direct",
                UserAId = meUserId,
                UserBId = otherUserId
            };

            return await _repo.CreateConversationAsync(newConv);
        }

        public async Task SendDirectMessageAsync(int senderId, int receiverId, string content)
        {
            var conversation = await GetOrCreateDirectConversationAsync(senderId, receiverId);

            var msg = new ChatMessage
            {
                ConversationId = conversation.ConversationId,
                SenderId = senderId,
                Content = content,
                SentAt = DateTime.Now
            };

            await _repo.AddMessageAsync(msg);
        }
        public async Task SendMessageAsync(int conversationId, int senderId, string content)
        {
            var msg = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content,
                SentAt = DateTime.Now
            };

            await _repo.AddMessageAsync(msg);
        }


        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId, int take = 100)
            => await _repo.GetMessagesAsync(conversationId, take);

        public async Task<IEnumerable<ChatConversation>> GetMyConversationsAsync(int userId)
            => await _repo.GetUserConversationsAsync(userId);
    }

}
