using BE_2911_CleanArchitechture.Logging;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Entites.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BE_2911_CleanArchitechture.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : BaseController
    {
        private readonly IChatServices _chatServices;
        private readonly IImageServices _imageService;
        private readonly IWebHostEnvironment _environment;

        public ChatController(
            IChatServices chatServices, 
            ICustomLogger logger, 
            IUserServices userServices,
            IImageServices imageService,
            IWebHostEnvironment environment)
            : base(logger, userServices)
        {
            _chatServices = chatServices;
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }

        // Lấy danh sách các cuộc trò chuyện của người dùng hiện tại
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = await GetUserIdFromTokenAsync();
            if (userId == 0) return Unauthorized();

            var result = await _chatServices.GetUserConversations(userId);
            return Ok(result);
        }

        // Lấy lịch sử tin nhắn của một cuộc trò chuyện cụ thể (có phân trang skip/take)
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [HttpGet("{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(long conversationId, [FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            var userId = await GetUserIdFromTokenAsync();
            if (userId == 0) return Unauthorized();

            try
            {
                var result = await _chatServices.GetMessages(conversationId, userId, skip, take);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Gửi tin nhắn mới đến một cuộc trò chuyện hoặc người dùng
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var userId = await GetUserIdFromTokenAsync();
            if (userId == 0) return Unauthorized();
            
            try
            {
                // 1. Kiểm tra xem Type gửi lên có nằm trong Enum không
                if (!Enum.IsDefined(typeof(MessageType), request.Type))
                {
                    return BadRequest($"Invalid MessageType value: {(int)request.Type}");
                }

                // 2. Validate chi tiết từng loại
                switch (request.Type)
                {
                    case MessageType.Text:
                        if (string.IsNullOrWhiteSpace(request.Content))
                            return BadRequest("Text message cannot be empty.");
                        // Chặn gửi đường dẫn file thô dưới dạng Text (theo yêu cầu của bạn)
                        if (request.Content.Contains("Uploads/Conversations/"))
                            return BadRequest("Cannot send raw file path as Text. Please set correct MessageType (Image/Voice/File).");
                        break;

                    case MessageType.Image:
                        if (string.IsNullOrEmpty(request.Content) || 
                            (!request.Content.Contains("/Images/") && !request.Content.Contains(@"\Images\")))
                            return BadRequest("Message of type 'Image' must reference a file in the 'Images' folder.");
                        break;

                    case MessageType.Voice:
                        if (string.IsNullOrEmpty(request.Content) || 
                            (!request.Content.Contains("/Voices/") && !request.Content.Contains(@"\Voices\")))
                            return BadRequest("Message of type 'Voice' must reference a file in the 'Voices' folder.");
                        break;

                    case MessageType.File:
                        if (string.IsNullOrEmpty(request.Content) || 
                            (!request.Content.Contains("/Files/") && !request.Content.Contains(@"\Files\")))
                            return BadRequest("Message of type 'File' must reference a file in the 'Files' folder.");
                        break;

                    default:
                        // Các Type khác (Video = 4, hoặc mở rộng sau này) chưa hỗ trợ Send qua đây
                        return BadRequest($"MessageType '{request.Type}' is not currently supported for sending.");
                }

                var result = await _chatServices.SendMessage(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // Tạo một nhóm chat mới với các thành viên được chỉ định
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [HttpPost("group")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            var userId = await GetUserIdFromTokenAsync();
            if (userId == 0) return Unauthorized();

            var result = await _chatServices.CreateGroupChat(userId, request.Title, request.MemberIds);
            return Ok(result);
        }

        /// <summary>
        /// Upload file đính kèm (Ảnh/Voice/Video...) - Unified Endpoint
        /// </summary>
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadAttachment([FromForm] UploadAttachmentRequest request, CancellationToken cancellationToken)
        {
            var userId = await GetUserIdFromTokenAsync();
            if (userId == 0) return Unauthorized();

            if (Request.Form.Files.Count == 0) return BadRequest("No file uploaded");

            try
            {
                // VALIDATION: Kiểm tra tính hợp lệ giữa MessageType và File ContentType
                if (request.MessageType == MessageType.Image)
                {
                    if (!request.File.ContentType.StartsWith("image/"))
                    {
                        return BadRequest("Invalid file format. MessageType 'Image' requires an image file.");
                    }
                }
                else if (request.MessageType == MessageType.Voice)
                {
                    if (!request.File.ContentType.StartsWith("audio/"))
                    {
                        return BadRequest("Invalid file format. MessageType 'Voice' requires an audio file.");
                    }
                }

                // Mapping MessageType (Enum Chat) -> TypeUploadImg (Enum Storage)
                int typeUploadStorage;
                string prefixIdentifier;

                switch (request.MessageType)
                {
                    case MessageType.Image:
                        typeUploadStorage = 4; // TypeUploadImg.Chat
                        prefixIdentifier = $"chat_{userId}";
                        break;
                    case MessageType.Voice:
                        typeUploadStorage = 5; // TypeUploadImg.Voice
                        prefixIdentifier = $"voice_{userId}";
                        break;
                    case MessageType.File:
                        typeUploadStorage = 6; // TypeUploadImg.ChatFile
                        prefixIdentifier = $"file_{userId}";
                        break;
                    default:
                        // Fallback hoặc báo lỗi nếu chưa hỗ trợ
                         _logger.LogInformation(userId.ToString(), $"Unsupported message type: {request.MessageType}");
                        return BadRequest("Unsupported message type for upload");
                }

                _logger.LogInformation(userId.ToString(), $"Upload request for Type: {request.MessageType}");

                // Gọi Service Upload dùng chung
                var uploaded = await _imageService.UploadImage(
                    Request,
                    prefixIdentifier,
                    userId,
                    typeUploadStorage,
                    request.ConversationId,
                    _environment.ContentRootPath,
                    cancellationToken);

                if (uploaded != null && uploaded.Count > 0)
                {
                    // Trả về URL và Type để Frontend tiện xử lý tiếp bước Send
                    return Ok(new { 
                        Url = uploaded.ElementAtOrDefault(0),
                        MessageType = request.MessageType
                    });
                }

                return BadRequest("Failed to upload file");
            }
            catch (Exception ex)
            {
                _logger.LogError(userId.ToString(), "UploadAttachment", ex);
                return BadRequest(ex.Message);
            }
        }



        /// <summary>
        /// Lưu lịch sử cuộc gọi
        /// </summary>
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [HttpPost("save-call")]
        public async Task<IActionResult> SaveCall([FromBody] SaveCallHistoryRequest request)
        {
            var userId = await GetUserIdFromTokenAsync();
            if (userId == 0) return Unauthorized();

            try
            {
                var result = await _chatServices.SaveCallHistory(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(userId.ToString(), "SaveCall", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Lấy tệp tin chat (Ảnh/Voice/File - Có kiểm tra quyền truy cập)
        /// </summary>
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [HttpGet("media/{**mediaPath}")]
        public async Task<IActionResult> GetChatMedia(string mediaPath)
        {
            var userId = await GetUserIdFromTokenAsync();
            if (userId == 0) return Unauthorized();

            try
            {
                var (stream, contentType) = await _chatServices.GetChatMediaStream(userId, mediaPath, _environment.ContentRootPath);
                return File(stream, contentType);
            }
            catch (System.IO.FileNotFoundException)
            {
                return NotFound("File not found");
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đã đọc của tin nhắn
        /// </summary>
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [HttpPut("message/{id}/read-status")]
        public async Task<IActionResult> UpdateMessageReadStatus(long id, [FromBody] UpdateReadStatusRequest request)
        {
            var userId = await GetUserIdFromTokenAsync();
            if (userId == 0) return Unauthorized();

            try
            {
                var result = await _chatServices.UpdateMessageReadStatus(userId, id, request.IsRead);
                return Ok(new { success = result, messageId = id, isRead = request.IsRead });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
           

    }

    public class CreateGroupRequest
    {
        public string Title { get; set; }
        public List<long> MemberIds { get; set; }
    }

    public class UploadAttachmentRequest
    {
        public int ConversationId { get; set; }
        public MessageType MessageType { get; set; }
        public IFormFile File { get; set; }
    }

    public class UpdateReadStatusRequest
    {
        public bool IsRead { get; set; }
    }
}
