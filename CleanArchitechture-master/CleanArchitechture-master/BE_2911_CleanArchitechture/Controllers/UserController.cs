using AutoMapper;
using CleanArchitecture.Application.Commands;
using CleanArchitecture.Application.Query;
using CleanArchitecture.Application.Query.Utilities;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<UserController> _logger; // Đổi tên từ ProductController sang UserController
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public UserController(IMediator mediator, IWebHostEnvironment environment, ILogger<UserController> logger, IConfiguration configuration, IMapper mapper)
        {
            _mediator = mediator;
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginQuery query)
        {
            try
            {
                _logger.LogInformation("Log   ||Login");
                var userList = await _mediator.Send(query);
                if (userList == "")
                {
                    this._logger.LogWarning("--------------------------Login failed for user: {Username}", query.Username);
                    var errors = new List<string> { "Invalid username or password." };
                    return Unauthorized(ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                    
                }
                var stringResult = new List<string> { userList };
                return Ok(new ApiResponse<List<string>>(stringResult));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                this._logger.LogError(ex, "--------------------------Internal server error: " + ex.Message);

                // Trả về mã lỗi 500 với thông điệp chi tiết
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("GetAllUser")]
        public async Task<IActionResult> GetAllUser()
        {
            try
            {
                //var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                _logger.LogInformation("Log   ||GetAllUser");
                var list = await _mediator.Send(new GetAllProductsQuery());
                return Ok(new ApiResponse<List<ProductDto>>(list));

            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                this._logger.LogError(ex, "--------------------------Internal server error : " + ex.Message);
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }

        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserCommand UserCommand)
        {
            try
            {
                try
                {
                    _logger.LogInformation("Log   ||RegisterUser");
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
                // Ghi log lỗi
                this._logger.LogError(ex, "--------------------------Internal server error : " + ex.Message);
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }
    }
}