using CleanArchitecture.Entites.Enums;

namespace CleanArchitecture.Application.Dtos
{
    public class MessageDto
    {
        public long Id { get; set; }
        public long ConversationId { get; set; }
        public long SenderId { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatar { get; set; }
        public string Content { get; set; }
        public string MessageType { get; set; } // String representation of enum
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsMine { get; set; } // Helper for frontend styling
    }

    public class SendMessageRequest
    {
        public long? ConversationId { get; set; } // If null, create new conversation (e.g. first msg to friend)
        public long? ReceiverId { get; set; } // Required if ConversationId is null
        public string Content { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;
    }
}
