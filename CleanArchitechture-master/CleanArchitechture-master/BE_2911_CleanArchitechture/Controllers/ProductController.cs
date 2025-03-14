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

        [HttpPost("GetListProduct")]
        [AllowAnonymous]
        [SwaggerOperation(Summary = "Lấy danh sách bài viết ",
                      Description = "")]

        public async Task<IActionResult> GetListProduct([FromBody] ApiRequest<string> request)
        {
            try
            {
                
                var list = await _mediator.Send(new GetAllProductsQuery(request.Skip, request.Take, request.RequestData));
                return Ok(new ApiResponse<List<ProductDto>>(list));
            }
            catch (Exception ex)
            {

                // Trả về mã lỗi 500 với thông điệp chi tiết
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));

            }
        }
        //Tạo bài viết review sản phẩm:
        //lấy ID tài khoản từ jwt, lưu trong DB, mục đích là để xác nhận bài viết này là tài khoản nào tạo ra
        //Tạo bài viết
        [HttpPost("CreateProduct")]
        [Authorize(Policy = "RequireAdminOrUserRole")]
        public async Task<IActionResult> Create(ProductCommand command)
        {
            long UserID = 0;
            try
            {
                //Check user ID 
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
                this._logger.LogInformation(UserID.ToString(), "CreateProduct");
                //Tạo sản phẩm review
                command.OwnerID = UserID;
                var list = await _mediator.Send(command);
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                //Tạo bài review
                //Lưu ảnh review
                var _uploadedfiles = Request.Form.Files;

                return Ok(new ApiResponse<Products>(list));

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
        //
        //[HttpGet("GetAll")]
        //public async Task<List<ProductEntity>> GetAll()
        //{
        //    var productlist = await this._container.Getall();
        //    if (productlist != null && productlist.Count > 0)
        //    {
        //        productlist.ForEach(item =>
        //        {
        //            item.productImage = GetImagebyProduct(item.Code);
        //        });
        //    }
        //    else
        //    {
        //        return new List<ProductEntity>();
        //    }
        //    return productlist;

        //}
        //[HttpGet("GetByCode")]
        //public async Task<ProductEntity> GetByCode(string Code)
        //{
        //    return await this._container.Getbycode(Code);

        //}

        //[HttpGet("Getbycategory")]
        //public async Task<List<ProductEntity>> Getbycategory(int Code)
        //{
        //    return await this._container.Getbycategory(Code);

        //}

        [HttpPost("UploadImage")]//method upload ảnh 
        [Authorize(Policy = "RequireAdminOrUserRole")]
        public async Task<ActionResult> UploadImage()
        {
            bool Results = false;
            long UserID = 0;
            try
            {
                //Check user ID 
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
                this._logger.LogInformation(UserID.ToString(),"UploadImage");
                var _uploadedfiles = Request.Form.Files;
                foreach (IFormFile source in _uploadedfiles)
                {
                    string Filename = source.FileName;
                    string Filepath = GetFilePath(Filename);

                    if (!System.IO.Directory.Exists(Filepath))
                    {
                        System.IO.Directory.CreateDirectory(Filepath);
                    }

                    string imagepath = Filepath + "\\image.png";

                    if (System.IO.File.Exists(imagepath))
                    {
                        System.IO.File.Delete(imagepath);
                    }
                    using (FileStream stream = System.IO.File.Create(imagepath))
                    {
                        await source.CopyToAsync(stream);
                        Results = true;
                    }


                }
                this._logger.LogInformation(UserID.ToString(), "Result: true");
                return Ok(new ApiResponse<Boolean>(Results));
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                this._logger.LogError(UserID.ToString(), "Internal server error: ", ex);

                // Trả về mã lỗi 500 với thông điệp chi tiết
                var errors = new List<string> { "Internal server error. Please try again later." };
                return StatusCode(500, ApiResponse<List<string>>.CreateErrorResponse(errors, false));
            }
        }

        //[HttpGet("get-image/{filename}")]//mothed nhận ảnh
        //public IActionResult GetImage(string filename)
        //{
        //    // Xác định đường dẫn đầy đủ của tệp ảnh
        //    string Filepath = GetFilePath(filename);
        //    string imagePath = Path.Combine(Filepath + "\\image.png");

        //    // Kiểm tra xem tệp ảnh có tồn tại không
        //    if (!System.IO.File.Exists(imagePath))
        //        return NotFound($"Image '{filename}' not found.");

        //    // Đọc dữ liệu ảnh từ tệp
        //    byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);

        //    // Trả về ảnh như phản hồi, nếu có yêu cầu resize ảnh, tệp ảnh nhỏ, cần thao tác header,xử lý ảnh
        //    //return new FileContentResult(imageBytes, $"image/{Path.GetExtension(filename).TrimStart('.')}");
        //    return new PhysicalFileResult(imagePath, $"image/{Path.GetExtension(filename).TrimStart('.')}");
        //}
        //[HttpGet("RemoveImage/{code}")]
        //public ResponseType RemoveImage(string code)
        //{
        //    string Filepath = GetFilePath(code);
        //    string Imagepath = Filepath + "\\image.png";
        //    try
        //    {
        //        if (System.IO.File.Exists(Imagepath))
        //        {
        //            System.IO.File.Delete(Imagepath);
        //        }
        //        return new ResponseType { Result = "pass", KyValue = code };
        //    }
        //    catch (Exception ext)
        //    {
        //        throw ext;
        //    }
        //}

        //[HttpPost("SaveProduct")]
        //public async Task<ResponseType> SaveProduct([FromBody] ProductEntity _product)
        //{
        //    return await this._container.SaveProduct(_product);
        //}


        [NonAction]
        private string GetFilePath(string ProductCode)
        {
            return this._environment.ContentRootPath + "\\Uploads\\Product\\" + ProductCode;
        }
        [NonAction]
        private string GetImagebyProduct(string productcode)
        {
            string ImageUrl = string.Empty;
            string HostUrl = "https://localhost:7118/";
            string Filepath = GetFilePath(productcode);
            string Imagepath = Filepath + "\\image.png";
            if (!System.IO.File.Exists(Imagepath))
            {
                ImageUrl = HostUrl + "/uploads/common/noimage.png";
            }
            else
            {
                ImageUrl = HostUrl + "/uploads/Product/" + productcode + "/image.png";
            }
            return ImageUrl;

        }

        //
    }
}
