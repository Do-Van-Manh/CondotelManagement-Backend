using CondotelManagement.Models;

namespace CondotelManagement.Services.Interfaces.Chat
{
    public interface IChatService
    {
        Task<ChatConversation> GetOrCreateDirectConversationAsync(int meUserId, int otherUserId);
        Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId, int take = 100);
        Task<IEnumerable<ChatConversation>> GetMyConversationsAsync(int userId);
        Task SendDirectMessageAsync(int senderId, int receiverId, string content);
        Task<IEnumerable<ConversationListItem>> GetMyConversationsWithDetailsAsync(int userId);
    }
    public class ConversationListItem
    {
        public int ConversationId { get; set; }
        public int UserAId { get; set; }
        public int UserBId { get; set; }
        public ChatMessage? LastMessage { get; set; }
        public int UnreadCount { get; set; }
        // Thông tin user đối phương
        public int? OtherUserId { get; set; }
        public string? OtherUserName { get; set; }
        public string? OtherUserImageUrl { get; set; }
    }
}