using AutoMapper;
using BE_2911_CleanArchitechture.Logging;
using CleanArchitecture.Application.Commands.Create;
using CleanArchitecture.Application.Commands.Update;
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
    public class ProductController : ControllerBase
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

        public ProductController(IMediator mediator, IWebHostEnvironment environment, ICustomLogger logger, IConfiguration configuration, IProductServices productServices,IUserServices userServices)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._environment = environment ?? throw new ArgumentNullException(nameof(environment));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._productServices = productServices ?? throw new ArgumentNullException(nameof(productServices));
            this._userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
        }
        //Tạo sản phẩm:
        //lấy ID tài khoản từ jwt, lưu trong DB, mục đích là để xác nhận bài viết này là tài khoản nào tạo ra
        //Tạo sản phẩm
        [HttpPost("CreateAProductReview")]
        [Authorize(Policy = "RequireAdminOrUserRole")]
                [SwaggerOperation(Summary = "Tạo các thông tin review sản phẩm+ảnh",
                      Description = "")]
        #region
        public async Task<IActionResult> CreateAProductReview([FromForm]  ProductCommand command)
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

                //B1: Tạo sản phẩm 

                this._logger.LogInformation(UserID.ToString(), "CreateProduct");
                command.OwnerID = UserID;
                int temp = 1;
                List<string> listRootImage = new List<string>();
                var _uploadedfiles = Request.Form.Files;
                foreach (IFormFile source in _uploadedfiles)
                {
                    //string Filename = source.FileName;
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
                        await source.CopyToAsync(stream);
                    }
                    listRootImage.Add(imagepath);
                    //Chỉ cho người dùng upload 5 ảnh
                    if (temp == 5)
                    {
                        break;
                    }
                    temp++;
                }
                command.ProductImage1 = listRootImage[0];
                command.ProductImage2= listRootImage[1];
                command.ProductImage3= listRootImage[2];
                command.ProductImage4= listRootImage[3];
                command.ProductImage5= listRootImage[4];
                ProductsDto aProduct;
                    aProduct = await _mediator.Send(command);
                // Kiểm tra xem việc tạo có thành công không
                if (aProduct == null)
                {
                    var errors = new List<string> { "Create product error" };
                    return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
                }
                this._logger.LogInformation(UserID.ToString(), "Result: true");
               

                return Ok(new ApiResponse<ProductsDto>(aProduct));

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
