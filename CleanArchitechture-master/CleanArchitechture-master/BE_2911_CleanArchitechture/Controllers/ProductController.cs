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
        private readonly IMapper _mapper;
        public ProductController(IMapper mapper,IMediator mediator, IWebHostEnvironment environment, ICustomLogger logger, IConfiguration configuration, IProductServices productServices,IUserServices userServices)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._environment = environment ?? throw new ArgumentNullException(nameof(environment));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._productServices = productServices ?? throw new ArgumentNullException(nameof(productServices));
            this._userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
            this._mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        //Tạo sản phẩm:
        //lấy ID tài khoản từ jwt, lưu trong DB, mục đích là để xác nhận bài viết này là tài khoản nào tạo ra
        //Tạo sản phẩm
        [HttpPost("CreateAProductReview")]
        [Authorize(Policy = "RequireAdminOrUserRole")]
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
                #region

                //B1: Tạo sản phẩm 

                this._logger.LogInformation(UserID.ToString(), "CreateProduct");
                command.OwnerID = UserID;
                Products aProduct = await _mediator.Send(command);
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                //ở trên là tạo bài nhưng chưa có ảnh sản phẩm
                //B2:Update tạo bài =>lưu ảnh (chỉ cho người dùng có thể upload tối đa 5 ảnh)
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
                #endregion
                //B3: Cập nhật đường dẫn ảnh lên server
                this._logger.LogInformation(UserID.ToString(), "UpDateImgProduct");
                aProduct.ProductImage1  = listRootImage[0];
                aProduct.ProductImage2 = listRootImage[1];
                aProduct.ProductImage3  = listRootImage[2];
                aProduct.ProductImage4  = listRootImage[3];
                aProduct.ProductImage5  = listRootImage[4];
                //Update 
                ProductcommandUpdate productcommandUpdate = new ProductcommandUpdate();
                productcommandUpdate.ProductId = aProduct.ProductId;
                productcommandUpdate.ReviewID= aProduct.ReviewID;
                productcommandUpdate.ProductName= aProduct.ProductName;
                productcommandUpdate.OwnerID= aProduct.OwnerID;
                productcommandUpdate.Price= aProduct.Price;
                productcommandUpdate.ProductImage1=aProduct.ProductImage1;
                productcommandUpdate.ProductImage2 = aProduct.ProductImage2;
                productcommandUpdate.ProductImage3=aProduct.ProductImage3;
                productcommandUpdate.ProductImage4=aProduct.ProductImage4;
                productcommandUpdate.ProductImage5=aProduct.ProductImage5;

                Products aProductUpdate = await _mediator.Send(productcommandUpdate);
                this._logger.LogInformation(UserID.ToString(), "Result: true");

                ProductDto aProductdto = _mapper.Map<ProductDto>(aProductUpdate);
                return Ok(new ApiResponse<ProductDto>(aProductdto));

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
