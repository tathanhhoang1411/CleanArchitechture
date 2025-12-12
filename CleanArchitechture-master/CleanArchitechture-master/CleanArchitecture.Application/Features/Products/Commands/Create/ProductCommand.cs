using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace CleanArchitecture.Application.Features.Products.Commands.Create
{

    public class ProductCommand : IRequest<ProductsDto>
    {
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public long OwnerID { get; set; }
        public int ReviewID { get; set; }
        public int Type { get; set; }
        public string? ProductImage1 { get; set; }
        public string? ProductImage2 { get; set; }
        public string? ProductImage3 { get; set; }
        public string? ProductImage4 { get; set; }
        public string? ProductImage5 { get; set; }
        public class CreateProductCommandHandler : IRequestHandler<ProductCommand, ProductsDto>
        {
            private readonly IProductServices _productServices;
            private readonly ILogger<CreateProductCommandHandler> _logger;
            public CreateProductCommandHandler(IProductServices productServices, ILogger<CreateProductCommandHandler> logger)
            {
                _productServices = productServices ?? throw new ArgumentNullException(nameof(productServices));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }
            public async Task<ProductsDto> Handle(ProductCommand command, CancellationToken cancellationToken)
            {
                ProductsDto prodDto = null;
                try
                {
                    _logger.LogInformation("CreateProductCommand starting for owner {OwnerID}", command.OwnerID);
                    DateTime dateTime = DateTime.Now;
                    long timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                    Entites.Entites.Product product = new Entites.Entites.Product
                    {
                        ProductName = command.ProductName,
                        Price = command.Price,
                        CreatedAt = dateTime,
                        OwnerID = command.OwnerID,
                        ReviewID = command.ReviewID,
                        ProductImage1 = command.ProductImage1,
                        ProductImage2 = command.ProductImage2,
                        ProductImage3 = command.ProductImage3,
                        ProductImage4 = command.ProductImage4,
                        ProductImage5 = command.ProductImage5,
                        Type = command.Type,
                    };

                    prodDto = await _productServices.Product_Create(product, cancellationToken);
                    _logger.LogInformation("CreateProductCommand completed for ProductId {ProductId}", product.ProductId);
                    return prodDto;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CreateProductCommand failed for owner {OwnerID}", command.OwnerID);
                    return prodDto;
                }
            }

        }
    }
}

