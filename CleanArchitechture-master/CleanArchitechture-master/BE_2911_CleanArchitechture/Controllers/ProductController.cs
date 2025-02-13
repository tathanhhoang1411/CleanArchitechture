using CleanArchitecture.Application.Commands;
using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Application.Query;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BE_2911_CleanArchitechture.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        //private IProductServices _productServices;
        //public ProductController(IProductServices productServices)
        //{
        //    _productServices = productServices;
        //}
        private readonly IWebHostEnvironment _environment;

        private readonly IMediator _mediator;
        private readonly ILogger<ProductController> _logger;
        private readonly IConfiguration _configuration;

        public ProductController(IMediator mediator, IWebHostEnvironment environment, ILogger<ProductController> logger, IConfiguration configuration)
        {
            this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this._environment = environment ?? throw new ArgumentNullException(nameof(environment));
            this._configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("GetListProduct")]
        public async Task<IActionResult> GetListProduct()
        {
            try
            {
                this._logger.LogInformation("Log   ||GetListProduct");
                var list = await _mediator.Send(new GetAllProductsQuery());
                return Ok(list);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                this._logger.LogError(ex, "--------------------------An error occurred while getting the product list.");

                // Trả về mã lỗi 500 với thông điệp chi tiết
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

        [HttpPost("CreateProduct")]
        public async Task<IActionResult> Create(CreateProductCommand command)
        {
            try
            {
            this._logger.LogInformation("Log   ||CreateProduct");
            return Ok(await _mediator.Send(command));

            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                this._logger.LogError(ex, "--------------------------An error occurred while getting the product list.");

                // Trả về mã lỗi 500 với thông điệp chi tiết
                return StatusCode(500, "Internal server error. Please try again later.");
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
        public async Task<ActionResult> UploadImage()
        {
            this._logger.LogInformation("----------------------------Log   ||UploadImage");
            bool Results = false;
            try
            {
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
                this._logger.LogInformation("----------------------------Log   ||UploadImage||True");
                return Ok(Results);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi
                this._logger.LogError(ex, "An error occurred while getting the product list.");

                // Trả về mã lỗi 500 với thông điệp chi tiết
                return StatusCode(500, "Internal server error. Please try again later.");
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
