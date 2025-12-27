using CleanArchitecture.Application.Dtos;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IChatServices
    {
        Task<List<ConversationDto>> GetUserConversations(long userId);
        Task<List<MessageDto>> GetMessages(long conversationId, long userId, int skip, int take);
        Task<MessageDto> SendMessage(long senderId, SendMessageRequest request);
        Task<ConversationDto> CreateGroupChat(long creatorId, string title, List<long> memberIds);
    }
}
