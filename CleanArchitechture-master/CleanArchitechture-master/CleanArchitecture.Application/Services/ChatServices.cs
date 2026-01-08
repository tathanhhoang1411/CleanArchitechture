using AutoMapper;
using System.Linq; // Added for Distinct and Where
using System.IO;
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

            // Mark 'IsMine' and setup MediaUrl
            foreach (var msg in messageDtos)
            {
                msg.IsMine = msg.SenderId == userId;
                
                // 4. Populate MediaUrl for Image/Voice
                // AutoMapper might map Enum to "Image"/"Voice" OR "1"/"3" depending on config.
                // We check both cases to be safe.
                if (msg.MessageType == "Image" || msg.MessageType == "Voice" || 
                    msg.MessageType == "1" || msg.MessageType == "3")
                {
                    // Ensure content starts with / for URL path
                    var contentPath = msg.Content.StartsWith("/") ? msg.Content : "/" + msg.Content;
                    msg.MediaUrl = $"/api/Chat/media{contentPath}";
                }
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

        public async Task<MessageDto> SaveCallHistory(long senderId, SaveCallHistoryRequest request)
        {
            // 1. Create a Message representing this call in the chat
            var message = new Message
            {
                ConversationId = request.ConversationId,
                SenderId = senderId,
                Content = request.Status == CallStatus.Missed ? "Cuộc gọi nhỡ" : 
                          request.Status == CallStatus.Rejected ? "Cuộc gọi bị từ chối" :
                          $"Cuộc gọi thoại ({request.Duration}s)",
                MessageType = MessageType.Voice, // Using Voice (3) as requested for call-related messages
                CreatedAt = request.StartedAt,
                IsRead = false
            };

            await _unitOfWork.Chat.SaveMessage(message);
            await _unitOfWork.CompleteAsync(); // Save to get MessageId

            // 2. Create CallHistory record
            var callHistory = new CallHistory
            {
                CallerId = senderId,
                ReceiverId = request.ReceiverId,
                CallType = request.CallType,
                Status = request.Status,
                StartedAt = request.StartedAt,
                EndedAt = request.EndedAt,
                Duration = request.Duration,
                MessageId = message.Id
            };

            await _unitOfWork.Chat.SaveCallHistory(callHistory);
            await _unitOfWork.CompleteAsync();

            // 3. Update Conversation LastMessageAt
            var conversation = await _unitOfWork.Chat.GetConversationById(request.ConversationId);
            if (conversation != null)
            {
                conversation.LastMessageAt = DateTime.Now;
                await _unitOfWork.CompleteAsync();
            }

            // 4. Return the message with call details mapped
            var savedMessage = (await _unitOfWork.Chat.GetMessages(request.ConversationId, 0, 1)).FirstOrDefault(m => m.Id == message.Id);
            var messageDto = _mapper.Map<MessageDto>(savedMessage);
            messageDto.IsMine = true;

            return messageDto;
        }

        public async Task<bool> UpdateMessageReadStatus(long userId, long messageId, bool isRead)
        {
            var message = await _unitOfWork.Chat.GetMessageById(messageId);
            if (message == null) throw new Exception("Message not found");

            // Verify user is in conversation
            var isParticipant = await _unitOfWork.Chat.IsUserInConversation(message.ConversationId, userId);
            if (!isParticipant) throw new Exception("Unauthorized to update read status for this message");

            // Update status
            message.IsRead = isRead;
            await _unitOfWork.CompleteAsync();

            return true;
        }

        public async Task<(Stream FileStream, string ContentType)> GetChatMediaStream(long userId, string mediaPath, string contentRootPath)
        {
            // 1. Decode URL
            mediaPath = System.Net.WebUtility.UrlDecode(mediaPath);

            // 2. Chuẩn hóa đường dẫn
            string normalizedPath = mediaPath.Replace('/', Path.DirectorySeparatorChar)
                                             .Replace('\\', Path.DirectorySeparatorChar);

            // 3. Kiểm tra format cơ bản
            var pathParts = normalizedPath.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length < 3 || !pathParts[0].Equals("Uploads", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid media path format");
            }

            // 4. Trích xuất Conversation ID
            long conversationId = 0;
            string convPrefix = "Conversation_";
            var convPart = pathParts.FirstOrDefault(p => p.StartsWith(convPrefix, StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrEmpty(convPart) ||
                !long.TryParse(convPart.Substring(convPrefix.Length), out conversationId))
            {
                throw new ArgumentException("Invalid conversation identifier in path");
            }

            // 5. CHECK QUYỀN
            // Chúng ta dùng Check trực tiếp Participant thay vì GetUserConversations để tối ưu Query (chỉ count 1 record thay vì list)
            var isParticipant = await _unitOfWork.Chat.IsUserInConversation(conversationId, userId);
            if (!isParticipant)
            {
                throw new UnauthorizedAccessException("User is not a participant in this conversation");
            }

            // 6. Kiểm tra file vật lý
            var fullPath = Path.Combine(contentRootPath, normalizedPath);

            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException("File not found on disk", fullPath);
            }

            // 7. Xác định Content Type
            var extension = Path.GetExtension(fullPath).ToLowerInvariant();
            var contentType = extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                ".wav" => "audio/wav",
                ".mp3" => "audio/mpeg",
                ".m4a" => "audio/mp4",
                ".pdf" => "application/pdf",
                ".doc" or ".docx" => "application/msword",
                ".xls" or ".xlsx" => "application/vnd.ms-excel",
                ".zip" or ".rar" => "application/zip",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };

            // Mở Stream ở chế độ Read, Share.Read để tránh lock file
            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return (stream, contentType);
        }
    }
}
