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

        public ChatController(IChatServices chatServices, ICustomLogger logger, IUserServices userServices)
            : base(logger, userServices)
        {
            _chatServices = chatServices;
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
    }

    public class CreateGroupRequest
    {
        public string Title { get; set; }
        public List<long> MemberIds { get; set; }
    }
}
