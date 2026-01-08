using CleanArchitecture.Application.Dtos;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IChatServices
    {
        Task<List<ConversationDto>> GetUserConversations(long userId);
        Task<List<MessageDto>> GetMessages(long conversationId, long userId, int skip, int take);
        Task<MessageDto> SendMessage(long senderId, SendMessageRequest request);
        Task<ConversationDto> CreateGroupChat(long creatorId, string title, List<long> memberIds);
        Task<MessageDto> SaveCallHistory(long senderId, SaveCallHistoryRequest request);
        Task<bool> UpdateMessageReadStatus(long userId, long messageId, bool isRead);
        Task<(System.IO.Stream FileStream, string ContentType)> GetChatMediaStream(long userId, string mediaPath, string contentRootPath);
    }
}
