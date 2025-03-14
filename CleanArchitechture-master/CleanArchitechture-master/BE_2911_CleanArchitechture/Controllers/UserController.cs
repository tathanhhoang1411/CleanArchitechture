using AutoMapper;
using BE_2911_CleanArchitechture.Logging;
using CleanArchitecture.Application.Commands;
using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Application.Query;
using CleanArchitecture.Application.Query.Utilities;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace BE_2911_CleanArchitecture.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly ICustomLogger _logger;
        private readonly IUserServices _userServices;
        public UserController(IMediator mediator, IWebHostEnvironment environment, ICustomLogger logger, IConfiguration configuration, IMapper mapper, IUserServices userServices)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
            this._environment = environment ?? throw new ArgumentNullException(nameof(environment));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        [HttpPost("Login")]
        [SwaggerOperation(Summary = "Đăng nhập để lấy thông tin JWT",
                      Description = "Sử dụng tên đăng nhập,email và mật khẩu để xác thực.")]
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
                var stringResult = new List<string> { userList };
                return Ok(new ApiResponse<List<string>>(stringResult));
            }
            catch (Exception ex)
            {

                // Trả về mã lỗi 500 với thông điệp chi tiết
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAllUser([FromBody] ApiRequest<string> request)
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
                else
                {
                    this._logger.LogInformation(UserID.ToString(), "Result: true");
                }
                #endregion
                _logger.LogInformation(UserID.ToString(), "GetAllUser");
                var list = await _mediator.Send(new GetAllUserQuery(request.Skip, request.Take,request.RequestData));
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                return Ok(new ApiResponse<List<UserDto>>(list));

            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                _logger.LogError(UserID.ToString(), "GetAllUser", ex);
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }

        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromBody] UserCommand UserCommand)
        {
            try
            {
                try
                {
                    Users user = await _mediator.Send(UserCommand);

                    //Mapper che giấu thuộc tính
                    // Ánh xạ Users thành UserDto và lưu vào biến
                    UserDto userDto = _mapper.Map<UserDto>(user);
                    userDto.Password = UserCommand.Password;
                    if (userDto == null)
                    {
                        var errors = new List<string> { "The username or Email already exists." };
                        return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));

                    }
                    return Ok(new ApiResponse<UserDto>(userDto));
                }
                catch (Exception ex)
                {

                    var errors = new List<string> { "The username or Email already exists." };
                    return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }

            }
            catch (Exception ex)
            {
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
    }
}