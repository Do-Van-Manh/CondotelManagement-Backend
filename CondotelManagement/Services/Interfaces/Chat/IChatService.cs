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

        // ← CHỈNH LẠI: Dùng class riêng ở ngoài, không nằm trong ChatService
        Task<IEnumerable<ConversationListItem>> GetMyConversationsWithDetailsAsync(int userId);
        Task AddMessageAsync(ChatMessage message);
    }

    // ← ĐẶT RA NGOÀI INTERFACE, PUBLIC ĐỂ DỄ DÙNG
    public class ConversationListItem
    {
        public int ConversationId { get; set; }
        public int UserAId { get; set; }
        public int UserBId { get; set; }
        public ChatMessage? LastMessage { get; set; }
        public int UnreadCount { get; set; }
    }
}