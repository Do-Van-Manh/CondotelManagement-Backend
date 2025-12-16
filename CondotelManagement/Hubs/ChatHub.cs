namespace CondotelManagement.Hub
{
    using CondotelManagement.Models;
    using CondotelManagement.Services.Interfaces.Chat;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.SignalR;
    using System.Security.Claims;

    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        // HÀM CHUNG – LẤY USER ID AN TOÀN
        private int GetCurrentUserId()
        {
            var claim = Context.User?.FindFirst("nameid")
                     ?? Context.User?.FindFirst(ClaimTypes.NameIdentifier)
                     ?? Context.User?.FindFirst("sub");

            if (claim == null || !int.TryParse(claim.Value, out int userId))
                throw new HubException("Unauthorized - Không tìm thấy user ID");

            return userId;
        }

        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        public async Task SendMessage(int conversationId, string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return;
            var senderId = GetCurrentUserId();

            // 1. Tạo và Lưu (Code cũ của bạn - Giữ nguyên)
            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow
            };
            await _chatService.AddMessageAsync(message);

            // 2. Chuẩn bị DTO (Code cũ của bạn - Giữ nguyên)
            var messageDto = new
            {
                messageId = message.MessageId,
                conversationId = message.ConversationId,
                senderId = message.SenderId,
                content = message.Content,
                sentAt = message.SentAt.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")
            };

            // --- 3. PHẦN SỬA ĐỔI: GỬI ĐÍCH DANH (REALTIME) ---

            // A. Tìm ID người nhận
            var receiverId = await _chatService.GetOtherUserIdInConversationAsync(conversationId, senderId);

            // B. Gửi cho người nhận (Nếu họ đang Online thì nhận được ngay)
            if (receiverId > 0)
            {
                // Convert ID sang String vì SignalR dùng String cho UserID
                await Clients.User(receiverId.ToString()).SendAsync("ReceiveMessage", messageDto);
            }

            // C. Gửi cho chính mình (Để đồng bộ nếu bạn đang mở 2 tab hoặc dùng điện thoại + web)
            await Clients.User(senderId.ToString()).SendAsync("ReceiveMessage", messageDto);
        }

        public async Task<int> GetOrCreateDirectConversation(int otherUserId)
        {
            var meId = GetCurrentUserId(); // Dùng hàm này → không còn Unauthorized

            var conv = await _chatService.GetOrCreateDirectConversationAsync(meId, otherUserId);
            await Groups.AddToGroupAsync(Context.ConnectionId, conv.ConversationId.ToString());
            return conv.ConversationId;
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }
}