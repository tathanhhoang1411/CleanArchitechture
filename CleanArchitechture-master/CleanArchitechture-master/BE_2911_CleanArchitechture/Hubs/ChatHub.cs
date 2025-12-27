using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace BE_2911_CleanArchitechture.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            // Thêm log này để xem ở cửa sổ chạy Backend (Visual Studio)
            System.Console.WriteLine($"[SignalR Hub] User connected: {userId} with ConnectionId: {Context.ConnectionId}");

            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            await base.OnConnectedAsync();
        }

        // Cho phép Client chủ động gia nhập nhóm nhận tin nhắn cá nhân
        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
        }
        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Conversation_{conversationId}");
        }

        // Hàm test: Bạn có thể gọi từ Console trình duyệt để xem mình có nhận được tin nhắn không
        public async Task TestPush(string message)
        {
            await Clients.Caller.SendAsync("ReceiveMessage", new
            {
                Content = message,
                SenderId = 0,
                ConversationId = 0,
                Timestamp = System.DateTime.Now
            });
        }
    }
}