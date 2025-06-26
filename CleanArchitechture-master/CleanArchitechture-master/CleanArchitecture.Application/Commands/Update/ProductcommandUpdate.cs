using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Application.Query.Utilities;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Commands.Update
{

    public class ProductcommandUpdate : IRequest<Products>
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
        public class CreateProductCommandHandler : IRequestHandler<ProductcommandUpdate, Products>
        {
            private readonly IProductServices _productServices;
            public CreateProductCommandHandler(IProductServices productServices)
            {
                _productServices = productServices;
            }
            public async Task<Products> Handle(ProductcommandUpdate command, CancellationToken cancellationToken)
            {
                //kiểm tra sản phẩm có tồn tại hay chưa
                var productDto = await _productServices.GetA_Products(command.ReviewID);
                if (productDto == null)
                {
                    return null;
                }
                else// có thì mới update được
                {
                    var product = new Products
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
                        ProductId= command.ProductId
                    };

                    await _productServices.Product_Update(product);
                    return product;
                }
            }

        }
    }
}

