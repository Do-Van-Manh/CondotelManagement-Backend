using CondotelManagement.Data;
using CondotelManagement.Models;
using CondotelManagement.Repositories.Interfaces.Chat;
using CondotelManagement.Services.Interfaces.Chat;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Repositories.Implementations.Chat
{
    public class ChatRepository : IChatRepository
    {
        private readonly CondotelDbVer1Context _ctx;

        public ChatRepository(CondotelDbVer1Context ctx)
        {
            _ctx = ctx;
        }

        public async Task<ChatConversation?> GetDirectConversationAsync(int userAId, int userBId)
        {
            return await _ctx.ChatConversations
                .Where(c => c.ConversationType == "direct")
                .FirstOrDefaultAsync(c =>
                    (c.UserAId == userAId && c.UserBId == userBId) ||
                    (c.UserAId == userBId && c.UserBId == userAId)
                );
        }

        public async Task<ChatConversation> CreateConversationAsync(ChatConversation conv)
        {
            await _ctx.ChatConversations.AddAsync(conv);
            await _ctx.SaveChangesAsync();
            return conv;
        }

        // ĐÃ CÓ SaveChangesAsync → TIN NHẮN SẼ LƯU DB LUÔN!
        public async Task AddMessageAsync(ChatMessage msg)
        {
            await _ctx.ChatMessages.AddAsync(msg);
            await _ctx.SaveChangesAsync(); // QUAN TRỌNG NHẤT – ĐÃ CÓ!!!
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId, int take = 100)
        {
            return await _ctx.ChatMessages
        .Where(m => m.ConversationId == conversationId)
        .OrderBy(m => m.SentAt)
        .ThenBy(m => m.MessageId) // ✅ QUAN TRỌNG: Nếu cùng thời gian thì sắp xếp theo ID
        .Take(take)
        .ToListAsync();
        }

        public async Task<IEnumerable<ChatConversation>> GetUserConversationsAsync(int userId)
        {
            return await _ctx.ChatConversations
                .Where(c => c.UserAId == userId || c.UserBId == userId)
                .ToListAsync();
        }

        // THÊM 2 METHOD MỚI – CẬP NHẬT LAST ACTIVITY + UNREAD COUNT
        // BỎ HOÀN TOÀN method này đi cũng được, hoặc để lại thì để trống
        public Task UpdateConversationLastActivityAsync(int conversationId, int lastMessageId)
        {
            // Không làm gì cả vì chúng ta sẽ tính real-time
            return Task.CompletedTask;
        }

        public async Task IncrementUnreadCountAsync(int conversationId, int senderId)
        {
            // Nếu bạn có bảng ConversationParticipants → dùng cái này (tốt nhất)
            /*
            var participant = await _ctx.ConversationParticipants
                .FirstOrDefaultAsync(p => p.ConversationId == conversationId && p.UserId != senderId);
            if (participant != null)
            {
                participant.UnreadCount += 1;
                await _ctx.SaveChangesAsync();
            }
            */

            // Nếu KHÔNG CÓ bảng participants → dùng cách đơn giản: TÍNH LẠI KHI LOAD
            // → Không cần làm gì ở đây cả! unreadCount sẽ được tính trong GetMyConversationsWithDetailsAsync
        }

        
    }
}