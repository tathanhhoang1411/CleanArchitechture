using CleanArchitecture.Application.Dtos;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IChatNotificationService
    {
        Task SendMessageToUser(long userId, MessageDto message);
        Task SendMessageToGroup(long conversationId, MessageDto message);
    }
}
