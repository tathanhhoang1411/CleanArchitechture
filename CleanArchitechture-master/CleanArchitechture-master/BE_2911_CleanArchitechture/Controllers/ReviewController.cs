using BE_2911_CleanArchitechture.Logging;
using CleanArchitecture.Application.Commands.Create;
using CleanArchitecture.Application.Commands.Select;
using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Application.Query;
using CleanArchitecture.Application.Query.Utilities;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;

namespace BE_2911_CleanArchitechture.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        //public ProductController(IProductServices productServices)
        //{
        //    _productServices = productServices;
        //}
        private readonly IWebHostEnvironment _environment;

        private readonly IProductServices _productServices;
        private readonly IUserServices _userServices;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly ICustomLogger _logger;
        public ReviewController(IMediator mediator, IWebHostEnvironment environment, ICustomLogger logger, IConfiguration configuration, IProductServices productServices,IUserServices userServices)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._environment = environment ?? throw new ArgumentNullException(nameof(environment));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._productServices = productServices ?? throw new ArgumentNullException(nameof(productServices));
            this._userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
        }

        [HttpPost("GetListReview")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Lấy danh sách review của tài khoản hiện tại",
                      Description = "")]
        [Authorize(Policy = "RequireAdminOrUserRole")]
        public async Task<IActionResult> GetListReview([FromBody] ApiRequest<string> request)
        {
            long UserID = 0;
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
                //Select review 
                this._logger.LogInformation(UserID.ToString(), "Reviewlist");
                QueryReview queryReview = new QueryReview();
                queryReview.str= request.RequestData;
                queryReview.userID = UserID;
                var list = await _mediator.Send(new ReviewQuerySelect(request.Skip, request.Take, queryReview));
                // Kiểm tra xem việc tạo có thành công không
                if (list.Count()==0)
                {
                    // Ghi log lỗi
                    this._logger.LogError(UserID.ToString(), "Review list error", null);
                    var errors = new List<string> { "Review list error" };
                    return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                return Ok(new ApiResponse<List<object>>(list));
            }
            catch (Exception ex)
            {

                // Trả về mã lỗi 500 với thông điệp chi tiết
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));

            }
        }
        //Tạo review 
        //lấy ID tài khoản từ jwt, lưu trong DB, mục đích là để xác nhận bài viết này là tài khoản nào tạo ra
        //Tạo bài viết
        [HttpPost("CreateAReview")]
        [Authorize(Policy = "RequireAdminOrUserRole")]
        public async Task<IActionResult> CreateAReview([FromBody]  ReviewCommand command)
        {
            long UserID = 0;
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
                //Tạo review

                this._logger.LogInformation(UserID.ToString(), "CreateAReview");
                command.OwnerID = UserID;
                Reviews aReview = await _mediator.Send(command);
                // Kiểm tra xem việc tạo có thành công không
                if (aReview == null)
                {
                    // Ghi log lỗi
                    this._logger.LogError(UserID.ToString(), "Review list error",null);
                    var errors = new List<string> { "Review list error" };
                    return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                return Ok(new ApiResponse<Reviews>(aReview));

            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                this._logger.LogError(UserID.ToString(), "Internal server error", ex);

                // Trả về mã lỗi 500 với thông điệp chi tiết
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }

        }
    }
}
