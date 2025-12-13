using BE_2911_CleanArchitechture.Logging;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Utilities;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace BE_2911_CleanArchitechture.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ICustomLogger _logger;
        protected readonly IUserServices _userServices;

        public BaseController(ICustomLogger logger, IUserServices userServices)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
        }

        protected async Task<long> GetUserIdFromTokenAsync()
        {
            try
            {
                string tokenJWT = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(tokenJWT))
                {
                    _logger.LogInformation("0", "Token is missing in request headers.");
                    return 0;
                }

                long userId = await _userServices.GetUserIDInTokenFromRequest(tokenJWT);
                _logger.LogInformation(userId.ToString(), "Check UserID in TokenJWT");

                if (userId == 0)
                {
                    _logger.LogError(userId.ToString(), "Result: false", null);
                }
                else
                {
                    _logger.LogInformation(userId.ToString(), "Result: true");
                }

                return userId;
            }
            catch (Exception ex)
            {
                _logger.LogError("0", "Error extracting UserID from token", ex);
                return 0;
            }
        }

        protected ObjectResult ForbiddenResponse(string message = "UserId not exist")
        {
            var errors = new List<string> { message };
            return StatusCode(403, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
        }
    }
}
