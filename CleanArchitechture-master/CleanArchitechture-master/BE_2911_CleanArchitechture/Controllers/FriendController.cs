using AutoMapper;
using BE_2911_CleanArchitechture.Logging;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Features.Comments.Query;
using CleanArchitecture.Application.Features.Friends.Commands.Create;
using CleanArchitecture.Application.Features.Users.Query;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Application.Utilities;
using CleanArchitecture.Entites.Entites;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;

namespace BE_2911_CleanArchitechture.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly ICustomLogger _logger;
        private readonly IUserServices _userServices;
        public FriendController(IMediator mediator
            , IWebHostEnvironment environment
            , ICustomLogger logger
            , IConfiguration configuration
            , IMapper mapper
            ,IUserServices userServices)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._environment = environment ?? throw new ArgumentNullException(nameof(environment));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
        }
        //Gửi lời mời kết bạn
        [HttpPost("send")]
        [SwaggerOperation(Summary = "Gửi kết bạn từ tài khoản hiện tại đến tài khoản khác thông qua userID",
                      Description = "")]
        #region
        public async Task<IActionResult> SendFriendRequest([FromQuery] long receiverId, CancellationToken cancellationToken)
        {
            long UserID=0;
            try
            {

                //Kiểm tra userID có tồn tại không 
                #region
                string tokenJWT = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                Task<long> UserIDTypeLong = _userServices.GetUserIDInTokenFromRequest(tokenJWT);
                UserID = await UserIDTypeLong;
                this._logger.LogInformation(UserID.ToString(), "Check UserID in TokenJWT");
                if (UserID == 0)
                {
                    this._logger.LogError(UserID.ToString(), "Result: false", null);
                    var errors = new List<string> { "UserId not exist" };
                    return StatusCode(403, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }
                else
                {
                    this._logger.LogInformation(UserID.ToString(), "Result: true");
                }
                #endregion
                //Select comment 
                this._logger.LogInformation(UserID.ToString(), "Friend request");
                FriendsDto aFriendDto = await _mediator.Send(new SendFriendRequestCommand(UserID,receiverId), cancellationToken);
                // Kiểm tra xem việc gửi kết bạn có thành công không
                if (aFriendDto.ReceiverId == 0)
                {
                    // Trả về thất bại
                    this._logger.LogInformation(UserID.ToString(), "Friend request fail");
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