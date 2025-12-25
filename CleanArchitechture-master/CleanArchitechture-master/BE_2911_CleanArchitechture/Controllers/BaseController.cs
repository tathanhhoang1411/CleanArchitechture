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

                string[] ArrayInfo = await _userServices.GetUserIDAndEmailInTokenFromRequest(tokenJWT);
                _logger.LogInformation(ArrayInfo[0], "Check UserID in TokenJWT");

                if (int.Parse(ArrayInfo[0]) == 0)
                {
                    _logger.LogError(ArrayInfo[0], "Result: false", null);
                }
                else
                {
                    _logger.LogInformation(ArrayInfo[0], "Result: true");
                }

                return long.Parse(ArrayInfo[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError("0", "Error extracting UserID from token", ex);
                return 0;
            }
        }
        protected async Task<string> GetEmailFromTokenAsync()
        {
            try
            {
                string tokenJWT = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(tokenJWT))
                {
                    _logger.LogInformation("0", "Token is missing in request headers.");
                    return "";
                }

                string[] ArrayInfo = await _userServices.GetUserIDAndEmailInTokenFromRequest(tokenJWT);
                _logger.LogInformation(ArrayInfo[0], "Check Email in TokenJWT");

                if (ArrayInfo[1] ==null || ArrayInfo[1] == "")
                {
                    _logger.LogError(ArrayInfo[1], "Result: false", null);
                }
                else
                {
                    _logger.LogInformation(ArrayInfo[1], "Result: true");
                }

                return ArrayInfo[1];
            }
            catch (Exception ex)
            {
                _logger.LogError("0", "Error extracting UserID from token", ex);
                return "";
            }
        }

        protected ObjectResult ForbiddenResponse(string message = "UserId not exist")
        {
            var errors = new List<string> { message };
            return StatusCode(403, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
        }
    }
}
