using CondotelManagement.Models;

namespace CondotelManagement.Services.Interfaces.Chat
{
    public interface IChatService
    {
        Task<ChatConversation> GetOrCreateDirectConversationAsync(int meUserId, int otherUserId);
        Task SendMessageAsync(int conversationId, int senderId, string content);
        Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId, int take = 100);
        Task<IEnumerable<ChatConversation>> GetMyConversationsAsync(int userId);

        Task SendDirectMessageAsync(int senderId, int receiverId, string content);
    }
}
