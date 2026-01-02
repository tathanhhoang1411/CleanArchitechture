using BE_2911_CleanArchitechture.Logging;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Interfaces;
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
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations()
        {
            var userId = await GetUserIdFromTokenAsync();
            if (userId == 0) return Unauthorized();

            var result = await _chatServices.GetUserConversations(userId);
            return Ok(result);
        }

        // Lấy lịch sử tin nhắn của một cuộc trò chuyện cụ thể (có phân trang skip/take)
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
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            var userId = await GetUserIdFromTokenAsync();
            if (userId == 0) return Unauthorized();
            
            try
            {
                var result = await _chatServices.SendMessage(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // Tạo một nhóm chat mới với các thành viên được chỉ định
        [HttpPost("group")]
        public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest request)
        {
            var userId = await GetUserIdFromTokenAsync();
            if (userId == 0) return Unauthorized();

            var result = await _chatServices.CreateGroupChat(userId, request.Title, request.MemberIds);
            return Ok(result);
        }

        /// <summary>
        /// Upload hình ảnh cho tin nhắn chat
        /// </summary>
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadChatImage([FromForm] long conversationId, CancellationToken cancellationToken)
        {
            var userId = await GetUserIdFromTokenAsync();
            if (userId == 0) return Unauthorized();

            List<string> uploaded = null;
            try
            {
                // Kiểm tra xem có file ảnh không
                if (Request.Form.Files.Count == 0)
                {
                    return BadRequest("No image file uploaded");
                }

                _logger.LogInformation(userId.ToString(), "UploadChatImage request");

                // Upload image(s). type=2 for chat images, using conversationId as imageId
                uploaded = await _imageService.UploadImage(
                    Request, 
                    $"chat_{userId}",  // identifier
                    userId,            // userId
                    2,                 // type: 2 for chat images
                    conversationId,    // imageId: use conversationId
                    _environment.ContentRootPath, 
                    cancellationToken);

                if (uploaded != null && uploaded.Count > 0)
                {
                    string imagePath = uploaded.ElementAtOrDefault(0);
                    _logger.LogInformation(userId.ToString(), $"Image uploaded: {imagePath}");
                    
                    return Ok(new { ImageUrl = imagePath });
                }

                return BadRequest("Failed to upload image");
            }
            catch (OperationCanceledException)
            {
                try { if (uploaded != null) await _imageService.DeleteUploadedFiles(uploaded, _environment.ContentRootPath, CancellationToken.None); } catch { }
                return StatusCode(499, "Client Closed Request");
            }
            catch (Exception ex)
            {
                try { if (uploaded != null) await _imageService.DeleteUploadedFiles(uploaded, _environment.ContentRootPath, CancellationToken.None); } catch { }
                _logger.LogError(userId.ToString(), "UploadChatImage", ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Lấy hình ảnh chat (Có kiểm tra quyền truy cập)
        /// </summary>
        [HttpGet("images/{**imagePath}")]
        public async Task<IActionResult> GetChatImage(string imagePath)
        {
            var userId = await GetUserIdFromTokenAsync();
            if (userId == 0) return Unauthorized();

            try
            {
                // Parse conversationId từ imagePath
                // Format: /uploads/2/{userId}/{conversationId}/image.jpg
                var pathParts = imagePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
                
                if (pathParts.Length < 4 || pathParts[0] != "uploads" || pathParts[1] != "2")
                {
                    return BadRequest("Invalid image path format");
                }

                long conversationId = long.Parse(pathParts[3]);

                // ✅ KIỂM TRA QUYỀN: User có phải là thành viên của conversation không?
                var conversations = await _chatServices.GetUserConversations(userId);
                bool isParticipant = conversations.Any(c => c.Id == conversationId);

                if (!isParticipant)
                {
                    _logger.LogError(userId.ToString(), $"Unauthorized access attempt to image: {imagePath}", null);
                    return Forbid(); // 403 Forbidden
                }

                // Construct full file path
                var fullPath = Path.Combine(_environment.ContentRootPath, imagePath);

                if (!System.IO.File.Exists(fullPath))
                {
                    _logger.LogError(userId.ToString(), $"Image not found: {imagePath}", null);
                    return NotFound();
                }

                // Detect content type from extension
                var extension = Path.GetExtension(fullPath).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".gif" => "image/gif",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };

                // Read and return file
                var fileBytes = await System.IO.File.ReadAllBytesAsync(fullPath);
                return File(fileBytes, contentType);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid conversation ID in path");
            }
            catch (Exception ex)
            {
                _logger.LogError(userId.ToString(), "GetChatImage error", ex);
                return StatusCode(500, "Error retrieving image");
            }
        }

    }

    public class CreateGroupRequest
    {
        public string Title { get; set; }
        public List<long> MemberIds { get; set; }
    }
}
