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

        // ============ WebRTC Signaling Methods ============
        
        /// <summary>
        /// Bắt đầu cuộc gọi (Video hoặc Voice)
        /// </summary>
        public async Task InitiateCall(string targetUserId, string callType)
        {
            var callerId = Context.UserIdentifier;
            var callId = Guid.NewGuid().ToString();
            
            System.Console.WriteLine($"[Call] {callerId} is calling {targetUserId} ({callType}) - CallId: {callId}");
            
            // Gửi thông báo cuộc gọi đến người nhận
            await Clients.Group($"User_{targetUserId}").SendAsync("IncomingCall", new
            {
                CallId = callId,
                CallerId = callerId,
                CallType = callType, // "video" or "voice"
                Timestamp = DateTime.UtcNow
            });
            
            // Confirm lại cho người gọi
            await Clients.Caller.SendAsync("CallInitiated", new { CallId = callId });
        }

        /// <summary>
        /// Trả lời cuộc gọi (Chấp nhận hoặc Từ chối)
        /// </summary>
        public async Task AnswerCall(string callId, string callerId, bool accept)
        {
            var receiverId = Context.UserIdentifier;
            
            System.Console.WriteLine($"[Call] {receiverId} answered call {callId}: {accept}");
            
            if (accept)
            {
                // Thông báo cho người gọi rằng cuộc gọi đã được chấp nhận
                await Clients.Group($"User_{callerId}").SendAsync("CallAccepted", new
                {
                    CallId = callId,
                    ReceiverId = receiverId
                });
            }
            else
            {
                // Thông báo từ chối
                await Clients.Group($"User_{callerId}").SendAsync("CallRejected", new
                {
                    CallId = callId,
                    ReceiverId = receiverId
                });
            }
        }

        /// <summary>
        /// Gửi WebRTC Offer (SDP)
        /// </summary>
        public async Task SendWebRTCOffer(string targetUserId, object offer)
        {
            var senderId = Context.UserIdentifier;
            System.Console.WriteLine($"[WebRTC] Sending Offer from {senderId} to {targetUserId}");
            
            await Clients.Group($"User_{targetUserId}").SendAsync("ReceiveWebRTCOffer", new
            {
                SenderId = senderId,
                Offer = offer
            });
        }

        /// <summary>
        /// Gửi WebRTC Answer (SDP)
        /// </summary>
        public async Task SendWebRTCAnswer(string targetUserId, object answer)
        {
            var senderId = Context.UserIdentifier;
            System.Console.WriteLine($"[WebRTC] Sending Answer from {senderId} to {targetUserId}");
            
            await Clients.Group($"User_{targetUserId}").SendAsync("ReceiveWebRTCAnswer", new
            {
                SenderId = senderId,
                Answer = answer
            });
        }

        /// <summary>
        /// Gửi ICE Candidate
        /// </summary>
        public async Task SendICECandidate(string targetUserId, object candidate)
        {
            var senderId = Context.UserIdentifier;
            
            await Clients.Group($"User_{targetUserId}").SendAsync("ReceiveICECandidate", new
            {
                SenderId = senderId,
                Candidate = candidate
            });
        }

        /// <summary>
        /// Kết thúc cuộc gọi
        /// </summary>
        public async Task EndCall(string targetUserId, string callId)
        {
            var senderId = Context.UserIdentifier;
            System.Console.WriteLine($"[Call] {senderId} ended call {callId} with {targetUserId}");
            
            await Clients.Group($"User_{targetUserId}").SendAsync("CallEnded", new
            {
                CallId = callId,
                SenderId = senderId
            });
        }
    }
}