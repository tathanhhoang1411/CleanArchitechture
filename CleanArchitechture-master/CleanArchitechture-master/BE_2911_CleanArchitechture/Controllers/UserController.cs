using AutoMapper;
using CleanArchitecture.Application.Query;
using CleanArchitecture.Application.Query.Utilities;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

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

        public UserController(IMediator mediator, IWebHostEnvironment environment, ILogger<UserController> logger, IConfiguration configuration,IMapper mapper)
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
                if (userList =="")
                {
                   this._logger.LogWarning("--------------------------Login failed for user: {Username}", query.Username);
                    return Unauthorized("--------------------------Invalid username or password.");
                }

                return Ok(userList);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                this._logger.LogError(ex, "--------------------------An error occurred while getting the product list.");

                // Trả về mã lỗi 500 với thông điệp chi tiết
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }
        [HttpPost("GetAllUser")]
        public async Task<IActionResult> GetAllUser([FromBody] ValidateToken validateToken)
        {
            try
            {
                _logger.LogInformation("Log   ||GetAllUser");
                string checkToken = await _mediator.Send(validateToken);
                if (checkToken == null)
                {
                    return Unauthorized("Invalid token. Please check and try again.");
                }
                var list = await _mediator.Send(new GetAllProductsQuery());
                return Ok(list);

            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                this._logger.LogError(ex, "--------------------------UnAuthorize");
                return StatusCode(401, "Internal server error. Please try again later.");
            }
        }
    }
}