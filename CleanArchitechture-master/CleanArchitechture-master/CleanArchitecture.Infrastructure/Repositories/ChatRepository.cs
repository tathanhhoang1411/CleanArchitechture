using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationContext _context;

        public ChatRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<Conversation> CreateConversation(Conversation conversation)
        {
            _context.Conversations.Add(conversation);
            return conversation;
        }

        public async Task<Conversation> GetConversationById(long conversationId)
        {
            return await _context.Conversations
                .Include(c => c.Participants)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(c => c.Id == conversationId);
        }

        public async Task<Conversation> GetExisting1v1Conversation(long user1Id, long user2Id)
        {
            // A 1v1 conversation is a conversation with exactly 2 participants,
            // where one participant is user1Id and the other is user2Id.
            // Special case: messaging self (user1Id == user2Id) -> exactly 1 participant? 
            // Usually systems prefer 2 participants even if same user, but DB constraint might prevent it.
            // Let's assume 1-1 with self has only 1 participant in our logic to avoid unique constraint.

            if (user1Id == user2Id)
            {
                return await _context.Conversations
                    .Where(c => c.Title == null && c.Participants.Count == 1 && c.Participants.Any(p => p.UserId == user1Id))
                    .Include(c => c.Participants)
                    .FirstOrDefaultAsync();
            }

            return await _context.Conversations
                .Where(c => c.Title == null && c.Participants.Count == 2 
                    && c.Participants.Any(p => p.UserId == user1Id) 
                    && c.Participants.Any(p => p.UserId == user2Id))
                .Include(c => c.Participants)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Conversation>> GetUserConversations(long userId)
        {
            // Get conversations where the user is a participant
            return await _context.Conversations
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .Include(c => c.Participants)
                    .ThenInclude(p => p.User)
                .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1)) // Preview last message
                    .ThenInclude(m => m.Sender)
                .OrderByDescending(c => c.LastMessageAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Message> SaveMessage(Message message)
        {
            _context.Messages.Add(message);
            
            // Update conversation LastMessageAt
            var conversation = await _context.Conversations.FindAsync(message.ConversationId);
            if (conversation != null)
            {
                conversation.LastMessageAt = message.CreatedAt;
                _context.Conversations.Update(conversation);
            }
            return message;
        }

        public async Task<List<Message>> GetMessages(long conversationId, int skip, int take)
        {
            return await _context.Messages
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .OrderBy(m => m.CreatedAt)
                // 1. Lấy thông tin User (Sender)
                .Include(m => m.Sender)
                    // 2. Chui tiếp vào bảng UserDetails để lấy FirstName, LastName
                    .ThenInclude(u => u.UserDetail)
                .Include(m => m.CallHistory) // 3. Lấy thông tin cuộc gọi nếu có
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<bool> AddParticipant(Participant participant)
        {
            try
            {
                _context.Participants.Add(participant);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsUserInConversation(long conversationId, long userId)
        {
            return await _context.Participants
                .AnyAsync(p => p.ConversationId == conversationId && p.UserId == userId);
        }

        public async Task<CallHistory> SaveCallHistory(CallHistory callHistory)
        {
            _context.CallHistories.Add(callHistory);
            return callHistory;
        }

        public async Task<Message> GetMessageById(long messageId)
        {
            return await _context.Messages.FindAsync(messageId);
        }
    }
}
