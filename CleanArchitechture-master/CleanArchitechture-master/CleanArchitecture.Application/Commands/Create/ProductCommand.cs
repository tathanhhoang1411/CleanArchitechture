using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Application.Query.Utilities;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Commands.Create
{

    public class ProductCommand : IRequest<ProductDto>
    {
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public long OwnerID { get; set; }
        public int ReviewID { get; set; }
        public string? ProductImage1 { get; set; }
        public string? ProductImage2 { get; set; }
        public string? ProductImage3 { get; set; }
        public string? ProductImage4 { get; set; }
        public string? ProductImage5 { get; set; }
        public class CreateProductCommandHandler : IRequestHandler<ProductCommand, ProductDto>
        {
            private readonly IProductServices _productServices;
            public CreateProductCommandHandler(IProductServices productServices)
            {
                _productServices = productServices??throw new ArgumentNullException(nameof(productServices));
            }
            public async Task<ProductDto> Handle(ProductCommand command, CancellationToken cancellationToken)
            {
                    ProductDto prodDto=null;
                try
                {
                    DateTime dateTime = DateTime.Now;
                    long timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                    Products product = new Products
                    {
                        ProductName = command.ProductName,
                        ProductId = timestamp.ToString(),
                        Price = command.Price,
                        CreatedAt = dateTime,
                        OwnerID = command.OwnerID,
                        ReviewID = command.ReviewID,
                        ProductImage1 = command.ProductImage1,
                        ProductImage2 = command.ProductImage2,
                        ProductImage3 = command.ProductImage3,
                        ProductImage4 = command.ProductImage4,
                        ProductImage5 = command.ProductImage5,
                    };

                    prodDto = await _productServices.Product_Create(product);
                    return prodDto;
                }
                catch  
                {
                    return prodDto;
                }
            }

        }
    }
}

