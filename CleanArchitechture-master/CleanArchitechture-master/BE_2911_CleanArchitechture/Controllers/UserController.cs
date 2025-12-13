using AutoMapper;
using BE_2911_CleanArchitechture.Logging;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Features.Users.Commands.Create;
using CleanArchitecture.Application.Features.Users.Commands.Update;
using CleanArchitecture.Application.Features.Users.Query;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Repository;
using CleanArchitecture.Application.Utilities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Diagnostics;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using UpdStatusUserCommand = CleanArchitecture.Application.Features.Users.Commands.Update.UpdStatusUserCommand;

namespace BE_2911_CleanArchitecture.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : BE_2911_CleanArchitechture.Controllers.BaseController
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _environment;
        private readonly IImageServices _imageService;
        public UserController(
            IMediator mediator
            , ICustomLogger logger
            , IWebHostEnvironment environment
            , IUserServices userServices
            , IImageServices imageService)
            : base(logger, userServices)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
            _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        }
        //Đăng nhập
        [HttpPost("Login")]
        [SwaggerOperation(Summary = "Đăng nhập để lấy thông tin JWT",
                      Description = "Sử dụng tên đăng nhập,email và mật khẩu để xác thực.")]
        #region
        public async Task<IActionResult> Login([FromBody] LoginQuery query, CancellationToken cancellationToken)
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
                //Check user ID 
                UserID = await GetUserIdFromTokenAsync();
                if (UserID == 0) return ForbiddenResponse();
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
        public async Task<IActionResult> RegisterUser([FromForm] UserCommand UserCommand, CancellationToken cancellationToken)
        {
            try
            {
                UserCommand.Username.Trim();
                UserCommand.Email.Trim();
                UserCommand.Password.Trim();
                List<string> listRootImage = await _imageService.UploadImage(Request, UserCommand.Email, 0, 1, 0, _environment.ContentRootPath, cancellationToken);
                if (listRootImage.Count > 0) UserCommand.Avatar = listRootImage.ElementAtOrDefault(0);
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
        #endregion
        //Tắt tài khoản user
        [Authorize(Policy = "RequireAdminRole")]
        [HttpPost("DeleteUser")]
        [SwaggerOperation(Summary = "Xóa tài khoản người dùng, chỉ admin mới có quyền",
                      Description = "")]
        #region
        public async Task<IActionResult> DeleteAUser([FromBody] UpdStatusUserCommand command, CancellationToken cancellationToken)
        {
            long UserID = 0;
            try
            {
                //Check user ID 
                //Check user ID 
                UserID = await GetUserIdFromTokenAsync();
                if (UserID == 0) return ForbiddenResponse();
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
        public async Task<IActionResult> ActiveAUser([FromBody] UpdStatusUserCommand command, CancellationToken cancellationToken)
        {
            long UserID = 0;
            try
            {
                //Check user ID 
                //Check user ID 
                UserID = await GetUserIdFromTokenAsync();
                if (UserID == 0) return ForbiddenResponse();
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
        public async Task<IActionResult> UpdPasswordUser([FromBody] UpdPasswordUserCommand command,CancellationToken cancellationToken)
        {
            long UserID = 0;
            try
            {
                //Check user ID 
                //Check user ID 
                UserID = await GetUserIdFromTokenAsync();
                if (UserID == 0) return ForbiddenResponse();
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
        public async Task<IActionResult> ResetPasswordUser([FromBody] ResetPasswordUserCommand command, CancellationToken cancellationToken)
        {
            long UserID = 0;
            try
            {
                //Check user ID 
                //Check user ID 
                UserID = await GetUserIdFromTokenAsync();
                if (UserID == 0) return ForbiddenResponse();
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