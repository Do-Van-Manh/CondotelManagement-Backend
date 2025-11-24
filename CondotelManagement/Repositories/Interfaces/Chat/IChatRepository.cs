using CondotelManagement.Models;

namespace CondotelManagement.Repositories.Interfaces.Chat
{
    public interface IChatRepository
    {
        Task<ChatConversation?> GetDirectConversationAsync(int userAId, int userBId);
        Task<ChatConversation> CreateConversationAsync(ChatConversation conv);
        Task AddMessageAsync(ChatMessage msg);
        Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId, int take = 100);
        Task<IEnumerable<ChatConversation>> GetUserConversationsAsync(int userId);

    }
}
