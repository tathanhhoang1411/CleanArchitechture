using CleanArchitecture.Entites.Entites;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IChatRepository
    {
        Task<Conversation> CreateConversation(Conversation conversation);
        Task<Conversation> GetConversationById(long conversationId);
        Task<Conversation> GetExisting1v1Conversation(long user1Id, long user2Id);
        Task<List<Conversation>> GetUserConversations(long userId);
        Task<Message> SaveMessage(Message message);
        Task<List<Message>> GetMessages(long conversationId, int skip, int take);
        Task<bool> AddParticipant(Participant participant);
        Task<bool> IsUserInConversation(long conversationId, long userId);
        Task<CallHistory> SaveCallHistory(CallHistory callHistory);
        Task<Message> GetMessageById(long messageId);
    }
}
