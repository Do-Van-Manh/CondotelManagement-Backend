using CondotelManagement.Data;
using CondotelManagement.Models;
using CondotelManagement.Repositories.Interfaces.Chat;
using Microsoft.EntityFrameworkCore;

namespace CondotelManagement.Repositories.Implementations.Chat
{
    
    public class ChatRepository : IChatRepository
    {
        private readonly CondotelDbVer1Context _ctx;
        public ChatRepository(CondotelDbVer1Context ctx) { _ctx = ctx; }

        public async Task<ChatConversation?> GetDirectConversationAsync(int userAId, int userBId)
        {
            return await _ctx.ChatConversations
                .Where(c => c.ConversationType == "direct")
                .Where(c =>
                    (c.UserAId == userAId && c.UserBId == userBId) ||
                    (c.UserAId == userBId && c.UserBId == userAId)
                )
                .FirstOrDefaultAsync();
        }

        public async Task<ChatConversation> CreateConversationAsync(ChatConversation conv)
        {
            await _ctx.ChatConversations.AddAsync(conv);
            await _ctx.SaveChangesAsync();
            return conv;
        }

        public async Task AddMessageAsync(ChatMessage msg)
        {
            _ctx.ChatMessages.Add(msg);
            await _ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(int conversationId, int take = 100)
        {
            return await _ctx.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.SentAt)
                .Take(take)
                .OrderBy(m => m.SentAt) 
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatConversation>> GetUserConversationsAsync(int userId)
        {
            return await _ctx.ChatConversations
                .Include(c => c.Messages)
                .Where(c =>
                    (c.UserAId == userId || c.UserBId == userId) ||
                    (c.ConversationType == "group") 
                )
                .ToListAsync();
        }
    }

}
