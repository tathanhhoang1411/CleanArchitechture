using AutoMapper;
using BE_2911_CleanArchitechture.Logging;
using CleanArchitecture.Application.Features.Users.Commands.Create;
using CleanArchitecture.Application.Features.Users.Commands.Update;
using CleanArchitecture.Application.Features.Users.Query;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Utilities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using UpdStatusUserCommand = CleanArchitecture.Application.Features.Users.Commands.Update.UpdStatusUserCommand;

namespace BE_2911_CleanArchitecture.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICustomLogger _logger;
        private readonly IUserServices _userServices;
        public UserController(IMediator mediator
            , ICustomLogger logger
            , IUserServices userServices)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        //Đăng nhập
        [HttpPost("Login")]
        [SwaggerOperation(Summary = "Đăng nhập để lấy thông tin JWT",
                      Description = "Sử dụng tên đăng nhập,email và mật khẩu để xác thực.")]
        #region
        public async Task<IActionResult> Login([FromBody] LoginQuery query)
        {
            try
            {
                var userList = await _mediator.Send(query);
                if (userList == "")
                {
                    var errors = new List<string> { "Invalid username or password." };
                    return Unauthorized(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                    
                }
                var token = userList; // Giả sử userList chứa token

                // Tạo đối tượng với key "AccessToken"
                var result = new
                {
                    AccessToken = token
                };

                return Ok(new ApiResponse<object>(result));
            }
            catch (Exception ex)
            {

                // Trả về mã lỗi 500 với thông điệp chi tiết
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        #endregion
        //Lấy danh sách tài khoản
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("GetAllUser")]
        #region
        public async Task<IActionResult> GetAllUser([FromQuery] int skip,[FromQuery] int take, [FromQuery] string requestData)
        {
            long UserID =0;
            try
            {
                //Check user ID 
                #region
                string tokenJWT = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                Task<long> UserIDTypeLong = _userServices.GetUserIDInTokenFromRequest(tokenJWT);
                UserID= await UserIDTypeLong;
                this._logger.LogInformation(UserID.ToString(), "Check UserID in TokenJWT");
                if (UserID == 0)
                {
                    this._logger.LogError(UserID.ToString(), "Result: false", null);
                    var errors = new List<string> { "UserId not exist" };
                    return StatusCode(403, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }
                    this._logger.LogInformation(UserID.ToString(), "Result: true");
                #endregion
                _logger.LogInformation(UserID.ToString(), "GetAllUser");
                ApiRequest<string> request = new ApiRequest<string>();
                request.Take = take;
                request.Skip = skip;
                request.RequestData = requestData;
                var list = await _mediator.Send(new GetAllUserQuery(request.Skip, request.Take,request.RequestData));
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                return Ok(new ApiResponse<List<UsersDto>>(list));

            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                _logger.LogError(UserID.ToString(), "GetAllUser", ex);
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        #endregion
        //Đăng ký tài khoản
        [HttpPost("RegisterUser")]
        #region
        public async Task<IActionResult> RegisterUser([FromBody] UserCommand UserCommand)
        {
            try
            {
                try
                {
                    UserCommand.Username.Trim();
                    UserCommand.Password.Trim();
                    UserCommand.Email.Trim();
                    UsersDto userDto = await _mediator.Send(UserCommand);

                    if (userDto == null)
                    {
                        var errors = new List<string> { "The username or Email already exists." };
                        return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));

                    }
                    if (userDto.Username == null)
                    {
                        var errors = new List<string> { "Created successfully, but error created or saved token. Please login" };
                        return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));

                    }
                    userDto.Password = UserCommand.Password;
                    return Ok(new ApiResponse<UsersDto>(userDto));
                }
                catch (Exception ex)
                {

                    var errors = new List<string> { "Internal server error. Please try again later." };
                    return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }

            }
            catch (Exception ex)
            {
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        #endregion
        //Tắt tài khoản user
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("DeleteUser")]
        [SwaggerOperation(Summary = "Xóa tài khoản người dùng, chỉ admin mới có quyền",
                      Description = "")]
        #region
        public async Task<IActionResult> DeleteAUser([FromBody] UpdStatusUserCommand command)
        {
            long UserID = 0;
            try
            {
                //Check user ID 
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
                _logger.LogInformation(UserID.ToString(), "DeleteAUser");
                UsersDto userDto = await _mediator.Send(command);
                // Kiểm tra xem tài khoản có tồn tại hay không
                if (userDto.Email == null)
                {
                    // Ghi log lỗi
                    this._logger.LogError(UserID.ToString(), "User not found", null);
                    var errors = new List<string> { "User not found" };
                    return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                return Ok(new ApiResponse<List<UsersDto>>(new List<UsersDto> { userDto }));

            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                _logger.LogError(UserID.ToString(), "DeleteAUser", ex);
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        #endregion
        //Kích hoạt lại tài khoản user
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("ActivateUser")]
        [SwaggerOperation(Summary = "Kích hoạt tài khoản người dùng, chỉ admin mới có quyền",
                      Description = "")]
        #region
        public async Task<IActionResult> ActiveAUser([FromBody] UpdStatusUserCommand command)
        {
            long UserID = 0;
            try
            {
                //Check user ID 
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
                _logger.LogInformation(UserID.ToString(), "DeleteAUser");
                UsersDto userDto = await _mediator.Send(command);
                // Kiểm tra xem tài khoản có tồn tại hay không
                if (userDto.Email == null)
                {
                    // Ghi log lỗi
                    this._logger.LogError(UserID.ToString(), "User not found", null);
                    var errors = new List<string> { "User not found" };
                    return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                return Ok(new ApiResponse<List<UsersDto>>(new List<UsersDto> { userDto }));

            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                _logger.LogError(UserID.ToString(), "DeleteAUser", ex);
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        #endregion
        //sửa password tài khoản user
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [HttpPost("UpdPasswordUser")]
        [SwaggerOperation(Summary = "Kích hoạt tài khoản người dùng",
                      Description = "")]
        #region
        public async Task<IActionResult> UpdPasswordUser([FromBody] UpdPasswordUserCommand command)
        {
            long UserID = 0;
            try
            {
                //Check user ID 
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
                _logger.LogInformation(UserID.ToString(), "UpdPasswordUser");
                UsersDto userDto = await _mediator.Send(command);
                // Kiểm tra xem tài khoản có tồn tại hay không
                if (userDto.Email == null)
                {
                    // Ghi log lỗi
                    this._logger.LogError(UserID.ToString(), "User not found", null);
                    var errors = new List<string> { "User not found" };
                    return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                return Ok(new ApiResponse<List<UsersDto>>(new List<UsersDto> { userDto }));

            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                _logger.LogError(UserID.ToString(), "UpdPasswordUser", ex);
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        #endregion
        //Đặt lại password tài khoản 
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("ResetPasswordUser")]
        [SwaggerOperation(Summary = "Đặt lại mật khẩu tài khoản người dùng, chỉ Admin mới có thể thao tác",
                      Description = "")]
        #region
        public async Task<IActionResult> ResetPasswordUser([FromBody] ResetPasswordUserCommand command)
        {
            long UserID = 0;
            try
            {
                //Check user ID 
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
                _logger.LogInformation(UserID.ToString(), "UpdPasswordUser");
                UsersDto userDto = await _mediator.Send(command);
                // Kiểm tra xem tài khoản có tồn tại hay không
                if (userDto.Email == null)
                {
                    // Ghi log lỗi
                    this._logger.LogError(UserID.ToString(), "User not found", null);
                    var errors = new List<string> { "User not found" };
                    return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                return Ok(new ApiResponse<List<UsersDto>>(new List<UsersDto> { userDto }));

            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                _logger.LogError(UserID.ToString(), "UpdPasswordUser", ex);
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        #endregion
    }
}