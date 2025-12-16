using CondotelManagement.Data;
using CondotelManagement.Models;
using CondotelManagement.Repositories.Interfaces.Chat;
using CondotelManagement.Services.Interfaces.Chat;
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
            // 1. Lấy conversations (đã Include UserA + UserB ở repo)
            var conversations = await _repo.GetUserConversationsAsync(userId);

            var result = new List<ConversationListItem>();

            foreach (var conv in conversations)
            {
                // 2. Lấy danh sách message
                var messages = await _repo.GetMessagesAsync(conv.ConversationId, 1000);
                var lastMsg = messages
                    .OrderByDescending(m => m.SentAt)
                    .ThenByDescending(m => m.MessageId)
                    .FirstOrDefault();

                // 3. Xác định user đối phương từ conversation (đã được Include)
                var otherUser = conv.UserAId == userId
                    ? conv.UserB         
                    : conv.UserA;

                // 4. Xác định otherUserId
                var otherUserId = conv.UserAId == userId ? conv.UserBId : conv.UserAId;

                // 5. Tính unread
                var unreadCount = messages.Count(m => m.SenderId != userId);

                // 6. Add vào result
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

            // 6. Sắp xếp theo tin nhắn mới nhất
            return result.OrderByDescending(x =>
                x.LastMessage?.SentAt ?? DateTime.MinValue
            );
        }

        public async Task AddMessageAsync(ChatMessage message)
        {
            // Gọi thẳng repo để lưu (có SaveChangesAsync bên trong)
            await _repo.AddMessageAsync(message);
        }
        public async Task<int> GetOtherUserIdInConversationAsync(int conversationId, int currentUserId)
        {
            return await _repo.GetOtherUserIdInConversationAsync(conversationId, currentUserId);
        }
    }
}