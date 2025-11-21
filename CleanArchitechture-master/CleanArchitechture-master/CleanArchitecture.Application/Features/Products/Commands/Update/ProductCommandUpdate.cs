using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Utilities;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Features.Products.Commands.Update
{

    public class ProductcommandUpdate : IRequest<ProductsDto>
    {
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public long OwnerID { get; set; }
        public int ReviewID { get; set; }
        public string ProductId { get; set; }
        public string? ProductImage1 { get; set; }
        public string? ProductImage2 { get; set; }
        public string? ProductImage3 { get; set; }
        public string? ProductImage4 { get; set; }
        public string? ProductImage5 { get; set; }
        public class CreateProductCommandHandler : IRequestHandler<ProductcommandUpdate, ProductsDto>
        {
            private readonly IProductServices _productServices;
            public CreateProductCommandHandler(IProductServices productServices)
            {
                _productServices = productServices;
            }
            public async Task<ProductsDto> Handle(ProductcommandUpdate command, CancellationToken cancellationToken)
            {

                try
                {
                    var product = new Entites.Entites.Product
                    {
                        ProductName = command.ProductName,
                        Price = command.Price,
                        OwnerID = command.OwnerID,
                        ReviewID = command.ReviewID,
                        ProductImage1 = command.ProductImage1,
                        ProductImage2 = command.ProductImage2,
                        ProductImage3 = command.ProductImage3,
                        ProductImage4 = command.ProductImage4,
                        ProductImage5 = command.ProductImage5,
                        ProductId = command.ProductId
                    };

                    ProductsDto proddto=await _productServices.Product_Update(product);
                    return proddto;
                }
                catch
                {
                    return new ProductsDto();
                }
            }

        }
    }
}

