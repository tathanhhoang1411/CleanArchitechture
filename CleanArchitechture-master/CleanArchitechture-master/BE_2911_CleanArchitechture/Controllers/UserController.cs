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
            if (UserCommand == null)
            {
                return BadRequest(ApiResponse<List<string>>.CreateErrorResponse(new List<string> { "Request body is required" }, false));
            }

            // Trim input safely
            UserCommand.Username = UserCommand.Username?.Trim();
            UserCommand.Email = UserCommand.Email?.Trim();
            UserCommand.Password = UserCommand.Password?.Trim();

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<List<string>>.CreateErrorResponse(new List<string> { "Invalid input" }, false));
            }

            List<string> listRootImage = null;

            try
            {
                UserCommand.Username = UserCommand.Username.Trim().ToLower().Replace(" ", "");
                UserCommand.Email = UserCommand.Email.Trim().ToLower().Replace(" ", "");
                UserCommand.Password = UserCommand.Password.Trim().ToLower().Replace(" ", "");

                // Check if avatar file for this email already exists
                bool isExist = await _imageService.IsImageExist(UserCommand.Email, 1, _environment.ContentRootPath, cancellationToken);
                if (isExist)
                {
                    return BadRequest(new ApiResponse<string>("Please change another email, because this email already exists."));
                }

                // Upload image(s)
                listRootImage = await _imageService.UploadImage(Request, UserCommand.Email, 0, 1, 0, _environment.ContentRootPath, cancellationToken);
                if (listRootImage != null && listRootImage.Count > 0)
                    UserCommand.Avatar = listRootImage.ElementAtOrDefault(0);

                // Create user via mediator (propagate cancellation)
                UsersDto userDto = await _mediator.Send(UserCommand, cancellationToken);

                // Validate mediator result
                if (userDto == null)
                {
                    // creation failed (e.g., duplicate)
                    await _imageService.DeleteUploadedFiles(listRootImage, _environment.ContentRootPath, cancellationToken);
                    var errors = new List<string> { "The username or Email already exists." };
                    return Conflict(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }

                if (string.IsNullOrWhiteSpace(userDto.Username) || string.IsNullOrWhiteSpace(userDto.Email))
                {
                    await _imageService.DeleteUploadedFiles(listRootImage, _environment.ContentRootPath, cancellationToken);
                    _logger.LogError(UserCommand?.Email ?? "0", "Mediator returned invalid user data", null);
                    var errors = new List<string> { "Created successfully, but resulting user data is invalid." };
                    return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }

                // Success
                userDto.Password = UserCommand.Password;
                return Ok(new ApiResponse<UsersDto>(userDto));
            }
            catch (OperationCanceledException)
            {
                // cleanup and return client closed request
                try { await _imageService.DeleteUploadedFiles(listRootImage, _environment.ContentRootPath, cancellationToken); } catch { }
                _logger.LogInformation(UserCommand?.Email ?? "0", "RegisterUser canceled");
                return StatusCode(499, "Client Closed Request");
            }
            catch (Exception ex)
            {
                // General error: rollback uploaded files and return 500
                try { await _imageService.DeleteUploadedFiles(listRootImage, _environment.ContentRootPath, cancellationToken); } catch { }
                _logger.LogError(UserCommand?.Email ?? "0", "RegisterUser error", ex);
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
        //Tìm kiếm người dùng
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [HttpGet("SearchUser")]
        [SwaggerOperation(Summary = "Tìm kiếm thông tin người dùng theo id/username/email",
                      Description = "Trả về thông tin user kèm userdetail")]
        #region
        public async Task<IActionResult> SearchUser([FromQuery] string id, CancellationToken cancellationToken)
        {
            try
            {
                // Validate JWT and get requester id
                long requesterId = await GetUserIdFromTokenAsync();
                if (requesterId == 0)
                {
                    return ForbiddenResponse();
                }

                if (string.IsNullOrWhiteSpace(id))
                {
                    var errors = new List<string> { "Parameter 'id' is required." };
                    return BadRequest(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }

                _logger.LogInformation(requesterId.ToString(), "SearchUser");
                var result = await _mediator.Send(new CleanArchitecture.Application.Features.Users.Query.GetUserWithDetailQuery(id), cancellationToken);
                if (result == null)
                {
                    var errors = new List<string> { "User not found." };
                    return NotFound(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }

                return Ok(new ApiResponse<CleanArchitecture.Application.Dtos.UserWithDetailDto>(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(id ?? "0", "SearchUser", ex);
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        #endregion
        // Upload avatar for current user
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [HttpPost("UploadAvatar")]
        [SwaggerOperation(Summary = "Upload ảnh đại diện cho tài khoản hiện tại",
                      Description = "Upload multipart/form-data với ảnh. Trả về Success! hoặc Fail!")]
        #region
        public async Task<IActionResult> UploadAvatar([FromForm] string requestEmail,CancellationToken cancellationToken)
        {
            long userId = 0;
            List<string> uploaded = null;
            try
            {
                userId = await GetUserIdFromTokenAsync();
                if (userId == 0) 
                { 
                return ForbiddenResponse();

                }
                else
                {
                    string email = await GetEmailFromTokenAsync();
                    if (email == "") return ForbiddenResponse();
                    if (email != requestEmail.ToLower().Trim()) return ForbiddenResponse();
                    this._logger.LogInformation(userId.ToString(), "UploadAvatar request");

                    // Upload image(s). type=1 for avatar, Id=0
                    uploaded = await _imageService.UploadImage(Request, email, userId, 1, 0, _environment.ContentRootPath, cancellationToken);
                    try
                    {
                        //Update user's avatar path if upload succeeded
                        if (uploaded != null && uploaded.Count > 0)
                        {
                            string avatarPath = uploaded.ElementAtOrDefault(0);
                            var updateCommand = new UpdateUserAvatarCommand
                            {
                                userID = userId,
                                avatarPath = avatarPath,
                                email = email
                            };
                            UsersDto updatedUser = await _mediator.Send(updateCommand, cancellationToken);
                            if (updatedUser.Avatar == "" || string.IsNullOrWhiteSpace(updatedUser.Avatar))
                            {
                                // Update failed: cleanup uploaded files
                                try { await _imageService.DeleteUploadedFiles(uploaded, _environment.ContentRootPath, cancellationToken); } catch { }
                                this._logger.LogError(userId.ToString(), "Failed to update user avatar in database", null);
                                return StatusCode(500, new ApiResponse<string>("Failed to update avatar."));
                            }
                        }

                    }
                    catch (OperationCanceledException ex)
                    {
                        try { if (uploaded != null) await _imageService.DeleteUploadedFiles(uploaded, _environment.ContentRootPath, CancellationToken.None); } catch { }
                        _logger.LogError(userId.ToString(), "UploadAvatar post-upload processing error", ex);
                        return StatusCode(499, "Client Closed Request");
                    }
                    if (uploaded != null && uploaded.Count > 0)
                    {
                        return Ok(new ApiResponse<string>("Success!"));
                    }

                    return Ok(new ApiResponse<string>("Fail!"));
                }
            }
            catch (OperationCanceledException)
            {
                try { if (uploaded != null) await _imageService.DeleteUploadedFiles(uploaded, _environment.ContentRootPath, CancellationToken.None); } catch { }
                return StatusCode(499, "Client Closed Request");
            }
            catch (Exception ex)
            {
                try { if (uploaded != null) await _imageService.DeleteUploadedFiles(uploaded, _environment.ContentRootPath, CancellationToken.None); } catch { }
                _logger.LogError(userId.ToString(), "UploadAvatar error", ex);
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
#endregion
        //Sửa thông tin tài khoản user
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [HttpPost("UpdInfoUser")]
        [SwaggerOperation(Summary = "Sửa thông tin chi tiết tài khoản người dùng",
                      Description = "")]
        #region
        public async Task<IActionResult> UpdateInfoUser([FromBody] UpdInfoUserCommand command, CancellationToken cancellationToken)
        {
            long UserID = 0;
            try
            {
                //Check user ID 
                UserID = await GetUserIdFromTokenAsync();
                if (UserID == 0 || UserID!= command.UserID  ) return ForbiddenResponse();
                _logger.LogInformation(UserID.ToString(), "UpdInfoUser");
                UserWithDetailDto userWithDetailDto = await _mediator.Send(command);
                // Kiểm tra xem tài khoản có tồn tại hay không
                if (userWithDetailDto.UserId == 0)
                {
                    // Ghi log lỗi
                    this._logger.LogError(UserID.ToString(), "User not found", null);
                    var errors = new List<string> { "User not found" };
                    return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                return Ok(new ApiResponse<List<UserWithDetailDto>>(new List<UserWithDetailDto> { userWithDetailDto }));

            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                _logger.LogError(UserID.ToString(), "UpdInfoUser", ex);
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        #endregion
    }
}