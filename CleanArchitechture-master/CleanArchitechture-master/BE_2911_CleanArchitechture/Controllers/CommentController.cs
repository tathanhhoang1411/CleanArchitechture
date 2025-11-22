using BE_2911_CleanArchitechture.Logging;
using CleanArchitecture.Application.Features.Comments.Commands.Create;
using CleanArchitecture.Application.Features.Comments.Query;
using CleanArchitecture.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using CleanArchitecture.Application.Utilities;
using CleanArchitecture.Application.Dtos;
using System.Threading;
using System.Collections.Generic;

namespace BE_2911_CleanArchitechture.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;

        private readonly ICommentServices _commentServices;
        private readonly IUserServices _userServices;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;
        private readonly ICustomLogger _logger;
        public CommentController(IMediator mediator, IWebHostEnvironment environment, ICustomLogger logger, IConfiguration configuration, ICommentServices commentServices, IUserServices userServices)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._environment = environment ?? throw new ArgumentNullException(nameof(environment));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._commentServices = commentServices ?? throw new ArgumentNullException(nameof(commentServices));
            this._userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
        }
        //Lấy danh sách comment bởi user đó
        [HttpGet("GetListCommentByOwner")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Lấy danh sách comment của tài khoản user",
                      Description = "")]
        [Authorize(Policy = "RequireUserRole")]
        #region
        public async Task<IActionResult> GetListComment( [FromQuery] int skip, [FromQuery] int take, [FromQuery] string requestData, CancellationToken cancellationToken)
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
                //Select comment 
                this._logger.LogInformation(UserID.ToString(), "Commentlist");
                ApiRequest<string> request = new ApiRequest<string>();
                request.Take = take;
                request.Skip = skip;
                request.RequestData = requestData;
                QueryEF queryComment = new QueryEF();
                queryComment.str = request.RequestData;
                queryComment.ID = UserID;

                // Pagination validation handled by PaginationValidationFilter globally

                var list = await _mediator.Send(new CommentQuerySelect(request.Skip, request.Take, queryComment), cancellationToken);

                // Kiểm tra xem việc lấy danh sách có thành công không
                if (list == null || list.Count() == 0)
                {
                    // Trả về 200 với mảng rỗng (hoặc thay đổi theo business)
                    this._logger.LogInformation(UserID.ToString(), "Comment list empty");
                    return Ok(new ApiResponse<List<CommentsDto>>(new List<CommentsDto>()));
                }
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                return Ok(new ApiResponse<List<CommentsDto>>(list));
            }
            catch (Exception ex)
            {

                // Trả về mã lỗi 500 với thông điệp chi tiết
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));

            }
        }
        #endregion
        //Lấy danh sách review theo ReviewID( theo bài viết đang xem)

        [HttpGet("GetListCommentByReviewID")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Lấy danh sách comment của bài viết đó",
              Description = "ID:  mã ID của bài review")]
        [Authorize(Policy = "RequireUserRole")]
        #region
        public async Task<IActionResult> GetListCommentByReviewID([FromQuery] int skip, [FromQuery] int take, [FromQuery] int requestData, CancellationToken cancellationToken)
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
                //Select comment 
                this._logger.LogInformation(UserID.ToString(), "Commentlist");
                ApiRequest<int> request = new ApiRequest<int>();
                request.Skip = skip;
                request.Take = take;
                request.Id = requestData;

                // Pagination validation handled by PaginationValidationFilter globally

                var list = await _mediator.Send(new CommentQuerySelectAllByReviewID(request.Skip, request.Take, request.Id), cancellationToken);
                // Kiểm tra xem việc lấy danh sách có thành công không
                if (list == null || list.Count() == 0)
                {
                    // Trả về 200 với mảng rỗng (hoặc thay đổi theo business)
                    this._logger.LogInformation(UserID.ToString(), "Comment list empty");
                    return Ok(new ApiResponse<List<CommentsDto>>(new List<CommentsDto>()));
                }
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                return Ok(new ApiResponse<List<CommentsDto>>(list));
            }
            catch (Exception ex)
            {

                // Trả về mã lỗi 500 với thông điệp chi tiết
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));

            }
        }
        #endregion
        //Tạo comment 
        //lấy ID tài khoản từ jwt, lưu trong DB, mục đích là để xác nhận bài viết này là tài khoản nào tạo ra
        [HttpPost("CreateAComment")]
        [Authorize(Policy = "RequireUserRole")]
        #region
        public async Task<IActionResult> CreateAComment([FromBody] CommentCommand command, CancellationToken cancellationToken)
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
                //Tạo comment

                this._logger.LogInformation(UserID.ToString(), "CreateAComment");
                command.OwnerID = UserID;
                CommentsDto aCommentDto = await _mediator.Send(command, cancellationToken);
                // Kiểm tra xem việc tạo có thành công không
                if (aCommentDto == null)
                {
                    // Ghi log lỗi
                    this._logger.LogError(UserID.ToString(), "Create comment error", null);
                    var errors = new List<string> { "Create comment error" };
                    return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                return Ok(new ApiResponse<CommentsDto>(aCommentDto));

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
        #endregion

    }
}
