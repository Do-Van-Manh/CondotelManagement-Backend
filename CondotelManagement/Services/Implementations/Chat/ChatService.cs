using CondotelManagement.Models;
using CondotelManagement.Repositories.Interfaces.Chat;
using CondotelManagement.Services.Interfaces.Chat;
using CondotelManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Services.Implementations.Chat
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _repo;
        private readonly CondotelDbVer1Context _context;
        public ChatService(IChatRepository repo, CondotelDbVer1Context context)
        {
            _repo = repo;
            _context = context;
        }

        // Các method cũ giữ nguyên...
        public async Task<ChatConversation> GetOrCreateDirectConversationAsync(int meUserId, int otherUserId)
        {
            var conv = await _repo.GetDirectConversationAsync(meUserId, otherUserId);
            if (conv != null) return conv;

            var newConv = new ChatConversation
            {
                ConversationType = "direct",
                UserAId = meUserId,
                UserBId = otherUserId,
                CreatedAt = DateTime.UtcNow
            };
            return await _repo.CreateConversationAsync(newConv);
        }

        public async Task SendDirectMessageAsync(int senderId, int receiverId, string content)
        {
            var conv = await GetOrCreateDirectConversationAsync(senderId, receiverId);
            var msg = new ChatMessage
            {
                ConversationId = conv.ConversationId,
                SenderId = senderId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow
            };
            await _repo.AddMessageAsync(msg);
        }

        public async Task SendMessageAsync(int conversationId, int senderId, string content)
        {
            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow
            };

            // DÙNG REPO ĐỂ LƯU → CÓ SaveChangesAsync BÊN TRONG!
            await _repo.AddMessageAsync(message);

            // Cập nhật LastActivity + tăng unread count cho người nhận
            await _repo.UpdateConversationLastActivityAsync(conversationId, message.MessageId);

            // Tăng unread cho người nhận (người không phải sender)
            await _repo.IncrementUnreadCountAsync(conversationId, senderId);
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId, int take = 100)
            => await _repo.GetMessagesAsync(conversationId, take);

        public async Task<IEnumerable<ChatConversation>> GetMyConversationsAsync(int userId)
            => await _repo.GetUserConversationsAsync(userId);

        // ← METHOD CHỈNH LẠI, KHÔNG LỖI int? → int VÀ ?? NỮA
        public async Task<IEnumerable<ConversationListItem>> GetMyConversationsWithDetailsAsync(int userId)
        {
            var conversations = await _repo.GetUserConversationsAsync(userId);
            var result = new List<ConversationListItem>();

            foreach (var conv in conversations)
            {
                // Lấy đủ tin nhắn để tính last + unread (có thể tối ưu sau)
                var messages = await _repo.GetMessagesAsync(conv.ConversationId, 1000);
                var lastMsg = messages
                    .OrderByDescending(m => m.SentAt)
                    .ThenByDescending(m => m.MessageId) // ✅ Thêm cái này
                    .FirstOrDefault();

                var unreadCount = messages.Count(m => m.SenderId != userId);

                // Xác định user đối phương
                var otherUserId = conv.UserAId == userId ? conv.UserBId : conv.UserAId;
                
                // Lấy thông tin user đối phương
                User? otherUser = null;
                if (otherUserId.HasValue)
                {
                    otherUser = await _context.Users.FindAsync(otherUserId.Value);
                }

                result.Add(new ConversationListItem
                {
                    ConversationId = conv.ConversationId,
                    UserAId = conv.UserAId ?? 0,
                    UserBId = conv.UserBId ?? 0,
                    LastMessage = lastMsg,
                    UnreadCount = unreadCount,
                    OtherUserId = otherUserId,
                    OtherUserName = otherUser?.FullName,
                    OtherUserImageUrl = otherUser?.ImageUrl
                });
            }

            // ← FIX lỗi ?? không áp dụng được cho DateTime? và int
            return result.OrderByDescending(x =>
                x.LastMessage?.SentAt ?? DateTime.MinValue
            );
        }
        public async Task AddMessageAsync(ChatMessage message)
        {
            // Gọi thẳng repo để lưu (có SaveChangesAsync bên trong)
            await _repo.AddMessageAsync(message);
        }
    }
}