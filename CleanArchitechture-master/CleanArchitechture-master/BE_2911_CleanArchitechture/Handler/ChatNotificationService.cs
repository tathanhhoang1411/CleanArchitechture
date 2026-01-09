using BE_2911_CleanArchitechture.Hubs;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace BE_2911_CleanArchitechture.Logging
{
    // Placing in Logging or a new Services folder in API
    public class ChatNotificationService : IChatNotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatNotificationService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendMessageToGroup(long conversationId, MessageDto message)
        {
             // If we had conversation groups
             await _hubContext.Clients.Group($"Conversation_{conversationId}").SendAsync("ReceiveMessage", message);
        }

        public async Task SendMessageToUser(long userId, MessageDto message)
        {
             await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveMessage", message);
        }

        public async Task NotifyMessageRead(long infoUserId, long conversationId, long messageId, long readerId)
        {
             // Gửi sự kiện cho người cần nhận thông báo (ví dụ: người gửi tin nhắn gốc)
             // Payload: Ai đã đọc (readerId), Đọc tin nào (messageId), Ở hội thoại nào (conversationId)
             await _hubContext.Clients.Group($"User_{infoUserId}").SendAsync("ReceiveReadStatus", new 
             { 
                 ConversationId = conversationId,
                 MessageId = messageId,
                 ReaderId = readerId,
                 ReadAt = DateTime.Now
             });
        }
    }
}
