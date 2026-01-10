using AutoMapper;
using BE_2911_CleanArchitechture.Logging;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Features.Comments.Query;
using CleanArchitecture.Application.Features.Friends.Commands.Create;
using CleanArchitecture.Application.Features.Friends.Commands.Update;
using CleanArchitecture.Application.Features.Friends.Query;
using CleanArchitecture.Application.Features.Users.Query;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Application.Utilities;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;

namespace BE_2911_CleanArchitechture.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        public FriendController(IMediator mediator
            , IWebHostEnvironment environment
            , ICustomLogger logger
            , IConfiguration configuration
            , IMapper mapper
            , IUserServices userServices)
            : base(logger, userServices)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._environment = environment ?? throw new ArgumentNullException(nameof(environment));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }
        //Gửi lời mời kết bạn
        [HttpPost("send")]
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [SwaggerOperation(Summary = "Gửi kết bạn từ tài khoản hiện tại đến tài khoản khác thông qua userID",
                      Description = "")]
        #region
        public async Task<IActionResult> SendFriendRequest([FromQuery] long receiverId, CancellationToken cancellationToken)
        {
            long UserID = 0;
            try
            {

                //Kiểm tra userID có tồn tại không 
                //Kiểm tra userID có tồn tại không 
                UserID = await GetUserIdFromTokenAsync();
                if (UserID == 0 || UserID == receiverId) return ForbiddenResponse();

                this._logger.LogInformation(UserID.ToString(), "Friend request starting ");
                FriendsDto aFriendDto = await _mediator.Send(new SendFriendRequestCommand(UserID, receiverId), cancellationToken);
                // Kiểm tra xem việc gửi kết bạn có thành công không
                if (aFriendDto.ReceiverId == 0)
                {
                    // Trả về thất bại
                    this._logger.LogInformation(UserID.ToString(), "Failed!");
                    return Ok(new ApiResponse<string>("Failed!"));
                }
                this._logger.LogInformation(UserID.ToString(), "Successed!");
                return Ok(new ApiResponse<FriendsDto>(aFriendDto));
            }
            catch (Exception ex)
            {

                // Trả về mã lỗi 500 với thông điệp chi tiết
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        #endregion

        //Lấy danh sách đã gửi lời mời kết bạn 
        [HttpGet("ListSendFriend")]
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [SwaggerOperation(Summary = "Lấy danh sách kết bạn",
                      Description = "status=1: lời mời chưa chấp nhận, status=2: lời mời đã được chấp nhận")]

        #region
        public async Task<IActionResult> ListSendFriend([FromQuery] int skip, int take, int status, CancellationToken cancellationToken)
        {
            long UserID = 0;
            try
            {

                //Kiểm tra userID có tồn tại không 
                UserID = await GetUserIdFromTokenAsync();
                if (UserID == 0) return ForbiddenResponse();
                //Select ListSendFriend 
                this._logger.LogInformation(UserID.ToString(), "ListSendFriend request");
                List<FriendsDto> listFriendDto = await _mediator.Send(new GetAllSendFriendRequestsQuery(skip, take, UserID, status), cancellationToken);
                // Kiểm tra xem việc gửi kết bạn có thành công không
                if (listFriendDto == null)
                {
                    // Trả về thất bại
                    this._logger.LogInformation(UserID.ToString(), "ListSendFriend request fail");
                    return Ok(new ApiResponse<string>("Fail!"));
                }
                this._logger.LogInformation(UserID.ToString(), "Success!");
                return Ok(new ApiResponse<List<FriendsDto>>(listFriendDto));
            }
            catch (Exception ex)
            {

                // Trả về mã lỗi 500 với thông điệp chi tiết
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        #endregion

        //Lấy danh sách lời mời kết bạn 
        [HttpGet("ListRequestFriend")]
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [SwaggerOperation(Summary = "Lấy danh sách lời mời kết bạn",
                      Description = "")]

        #region
        public async Task<IActionResult> ListRequestFriend([FromQuery] int skip, int take, CancellationToken cancellationToken)
        {
            long UserID = 0;
            try
            {

                //Kiểm tra userID có tồn tại không 
                UserID = await GetUserIdFromTokenAsync();
                if (UserID == 0) return ForbiddenResponse();
                //Select ListSendFriend 
                this._logger.LogInformation(UserID.ToString(), "ListRequestFriend request");
                List<FriendsDto> listFriendDto = await _mediator.Send(new GetAllSendFriendRequestsQuery(skip, take, UserID, 11), cancellationToken);
                // Kiểm tra xem việc lấy DS lời mời kết bạn có thành công không
                if (listFriendDto == null)
                {
                    // Trả về thất bại
                    this._logger.LogInformation(UserID.ToString(), "ListRequestFriend request fail");
                    return Ok(new ApiResponse<string>("Fail!"));
                }
                this._logger.LogInformation(UserID.ToString(), "Success!");
                return Ok(new ApiResponse<List<FriendsDto>>(listFriendDto));
            }
            catch (Exception ex)
            {

                // Trả về mã lỗi 500 với thông điệp chi tiết
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        #endregion

        //Chấp nhận/ xóa lời mời kết bạn
        [HttpPost("set")]
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [SwaggerOperation(Summary = "Chấp nhận: 2- từ chối( xóa: 0) lời mời kết bạn từ tài khoản hiện tại đến tài khoản khác thông qua userID",
                      Description = "")]
        #region
        public async Task<IActionResult> SetFriendRequest([FromQuery] long receiverId, int status, CancellationToken cancellationToken)
        {
            long UserID = 0;
            try
            {
                //Kiểm tra userID có tồn tại không 
                UserID = await GetUserIdFromTokenAsync();
                if (UserID == 0 || UserID == receiverId) return ForbiddenResponse();

                this._logger.LogInformation(UserID.ToString(), "Setting friend request");
                FriendsDto aFriendDto = await _mediator.Send(new SetFriendRequestCommand(receiverId, UserID, status), cancellationToken);
                // Kiểm tra xem việc gửi kết bạn có thành công không
                if (aFriendDto.ReceiverId == 0)
                {
                    // Trả về thất bại
                    this._logger.LogInformation(UserID.ToString(), "Fail!");
                    return Ok(new ApiResponse<string>("Fail!"));
                }
                this._logger.LogInformation(UserID.ToString(), "Success!");
                return Ok(new ApiResponse<FriendsDto>(aFriendDto));
            }
            catch (Exception ex)
            {

                // Trả về mã lỗi 500 với thông điệp chi tiết
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        #endregion
    }
}