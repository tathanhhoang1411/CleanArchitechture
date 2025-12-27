using AutoMapper;
using System.Linq; // Added for Distinct and Where
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Enums;
using CleanArchitecture.Entites.Interfaces;


namespace CleanArchitecture.Application.Services
{
    public class ChatServices : IChatServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IChatNotificationService _notificationService;

        public ChatServices(IUnitOfWork unitOfWork, IMapper mapper, IChatNotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notificationService = notificationService;
        }

        public async Task<List<ConversationDto>> GetUserConversations(long userId)
        {
            var conversations = await _unitOfWork.Chat.GetUserConversations(userId);
            return _mapper.Map<List<ConversationDto>>(conversations);
        }

        public async Task<List<MessageDto>> GetMessages(long conversationId, long userId, int skip, int take)
        {
            // Verify user is in conversation
            var isParticipant = await _unitOfWork.Chat.IsUserInConversation(conversationId, userId);
            if (!isParticipant)
            {
                throw new Exception("User is not a participant in this conversation");
            }

            var messages = await _unitOfWork.Chat.GetMessages(conversationId, skip, take);
            var messageDtos = _mapper.Map<List<MessageDto>>(messages);

            // Mark 'IsMine'
            foreach (var msg in messageDtos)
            {
                msg.IsMine = msg.SenderId == userId;
            }

            return messageDtos;
        }

        public async Task<MessageDto> SendMessage(long senderId, SendMessageRequest request)
        {
            long conversationId = 0;

            // 1. If ConversationId is null, check if existing 1v1 conversation exists or create new one
            if (request.ConversationId.HasValue)
            {
                conversationId = request.ConversationId.Value;
                // Verify sender is in conversation
                if (!await _unitOfWork.Chat.IsUserInConversation(conversationId, senderId))
                {
                    throw new Exception("User is not in this conversation");
                }
            }
            else if (request.ReceiverId.HasValue)
            {
                // Find existing 1-1 conversation
                var existingConversation = await _unitOfWork.Chat.GetExisting1v1Conversation(senderId, request.ReceiverId.Value);
                if (existingConversation != null)
                {
                    conversationId = existingConversation.Id;
                }
                else
                {
                    var conversation = new Conversation
                    {
                        Title = null, // 1v1
                        CreatedAt = DateTime.Now
                    };
                    await _unitOfWork.Chat.CreateConversation(conversation);
                    await _unitOfWork.CompleteAsync(); // Save to get Id
                    
                    conversationId = conversation.Id;

                    // Add Participants
                    await _unitOfWork.Chat.AddParticipant(new Participant { ConversationId = conversationId, UserId = senderId });
                    
                    // Only add receiver if it's different from sender
                    if (request.ReceiverId.Value != senderId)
                    {
                        await _unitOfWork.Chat.AddParticipant(new Participant { ConversationId = conversationId, UserId = request.ReceiverId.Value });
                    }
                    await _unitOfWork.CompleteAsync(); // Commit participants
                }
            }
            else
            {
                throw new Exception("ConversationId or ReceiverId must be provided");
            }

            // 2. Create and Save Message
            var message = new Message
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = request.Content,
                MessageType = request.Type,
                CreatedAt = DateTime.Now,
                IsRead = false
            };

            await _unitOfWork.Chat.SaveMessage(message);
            await _unitOfWork.CompleteAsync();

            // 3. Map to DTO
            var savedMessage = (await _unitOfWork.Chat.GetMessages(conversationId, 0, 1)).FirstOrDefault();
            var messageDto = _mapper.Map<MessageDto>(savedMessage);
            messageDto.IsMine = true;

            // 4. Send Notification via Interface
            var conversationEntity = await _unitOfWork.Chat.GetConversationById(conversationId);
            foreach(var p in conversationEntity.Participants)
            {
                 // Notify each user
                 await _notificationService.SendMessageToUser(p.UserId, messageDto);
            }

            return messageDto;
        }

        public async Task<ConversationDto> CreateGroupChat(long creatorId, string title, List<long> memberIds)
        {
            var conversation = new Conversation
            {
                Title = title,
                CreatedAt = DateTime.Now
            };
            await _unitOfWork.Chat.CreateConversation(conversation);
            await _unitOfWork.CompleteAsync();

            // Filter memberIds: unique and must not include creatorId
            var finalMemberIds = memberIds.Distinct().Where(id => id != creatorId).ToList();

            // Add creator
            await _unitOfWork.Chat.AddParticipant(new Participant { ConversationId = conversation.Id, UserId = creatorId, JoinedAt = DateTime.Now });
            
            // Add other members
            foreach(var memberId in finalMemberIds)
            {
                await _unitOfWork.Chat.AddParticipant(new Participant { ConversationId = conversation.Id, UserId = memberId, JoinedAt = DateTime.Now });
            }
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<ConversationDto>(conversation);
        }
    }
}
