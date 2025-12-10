using AutoMapper;
using BE_2911_CleanArchitechture.Logging;
using CleanArchitecture.Application.Features.Products.Commands.Create;
using CleanArchitecture.Application.Features.Products.Commands.Update;
using CleanArchitecture.Application.Features.Products.Query;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Features.Users.Commands.Create;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Utilities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Annotations;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BE_2911_CleanArchitechture.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IUserServices _userServices;
        private readonly IMediator _mediator;
        private readonly ILogger<ProductController> _ilogger;

        public ProductController(IMediator mediator
            , IWebHostEnvironment environment
            , ICustomLogger logger
            ,IUserServices userServices
            , ILogger<ProductController> ilogger)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._environment = environment ?? throw new ArgumentNullException(nameof(environment));
            this._userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
            _ilogger = ilogger ?? throw new ArgumentNullException(nameof(ilogger));
        }

        [HttpPost("CreateAProductReview")]
        [Authorize(Policy = "RequireAdminOrUserRole")]
        [SwaggerOperation(Summary = "Tạo các thông tin review sản phẩm+ảnh",
                      Description = "")]
        public async Task<IActionResult> CreateAProductReview([FromForm]  ProductCommand command, CancellationToken cancellationToken)
        {
            long UserID = 0;
            var sw = Stopwatch.StartNew();
            try
            {
                string tokenJWT = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                Task<long> UserIDTypeLong = _userServices.GetUserIDInTokenFromRequest(tokenJWT);
                UserID = await UserIDTypeLong;
                _ilogger.LogInformation("Check UserID in TokenJWT: {UserID}", UserID);
                if (UserID == 0)
                {
                    _ilogger.LogWarning("UserId not exist in token");
                    var errors = new List<string> { "UserId not exist" };
                    return StatusCode(403, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }

                _ilogger.LogInformation("CreateProduct start for user {UserID}", UserID);
                command.OwnerID = UserID;
                int temp = 1;
                List<string> listRootImage = new List<string>();
                var _uploadedfiles = Request.Form.Files;
                foreach (IFormFile source in _uploadedfiles)
                {
                    string Filename = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds().ToString();
                    string Filepath = GetFilePath(UserID.ToString(), "Post", command.ReviewID);

                    if (!System.IO.Directory.Exists(Filepath))
                    {
                        System.IO.Directory.CreateDirectory(Filepath);
                    }

                    string imagepath = Filepath + "\\" + temp + ".jpg";

                    if (System.IO.File.Exists(imagepath))
                    {
                        System.IO.File.Delete(imagepath);
                    }
                    using (FileStream stream = System.IO.File.Create(imagepath))
                    {
                        await source.CopyToAsync(stream, cancellationToken);
                    }
                    listRootImage.Add(imagepath);
                    if (temp == 5)
                    {
                        break;
                    }
                    temp++;
                }

                if (listRootImage.Count > 0) command.ProductImage1 = listRootImage.ElementAtOrDefault(0);
                if (listRootImage.Count > 1) command.ProductImage2 = listRootImage.ElementAtOrDefault(1);
                if (listRootImage.Count > 2) command.ProductImage3 = listRootImage.ElementAtOrDefault(2);
                if (listRootImage.Count > 3) command.ProductImage4 = listRootImage.ElementAtOrDefault(3);
                if (listRootImage.Count > 4) command.ProductImage5 = listRootImage.ElementAtOrDefault(4);

                var aProduct = await _mediator.Send(command, cancellationToken);
                sw.Stop();
                _ilogger.LogInformation("CreateAProductReview completed in {ElapsedMs}ms for user {UserID}", sw.ElapsedMilliseconds, UserID);

                if (aProduct == null)
                {
                    var errors = new List<string> { "Create product error" };
                    return StatusCode(404, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }

                return Ok(new ApiResponse<ProductsDto>(aProduct));
            }
            catch (OperationCanceledException)
            {
                _ilogger.LogWarning("CreateAProductReview canceled for user {UserID}", UserID);
                return StatusCode(499, "Client Closed Request");
            }
            catch (Exception ex)
            {
                _ilogger.LogError(ex, "CreateAProductReview error for user {UserID}", UserID);
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }

        [NonAction]
        private string GetFilePath(string userID,string flag,int reviewId)
        {
            if (flag!="Post")
            {
                return this._environment.ContentRootPath + "\\Uploads\\UserID_" + userID+"\\Comment_"+ reviewId;

            }
            return this._environment.ContentRootPath + "\\Uploads\\UserID_" + userID+"\\Review_"+ reviewId;
        }
    }
}
