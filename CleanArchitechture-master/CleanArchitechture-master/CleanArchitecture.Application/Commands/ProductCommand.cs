using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Commands
{

    public class ProductCommand : IRequest<Products>
    {
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public long OwnerID { get; set; }
        public class CreateProductCommandHandler : IRequestHandler<ProductCommand, Products>
        {
            private readonly IProductServices _productServices;
            public CreateProductCommandHandler(IProductServices productServices)
            {
                _productServices = productServices;
            }
            public async Task<Products> Handle(ProductCommand command, CancellationToken cancellationToken)
            {
                DateTime dateTime = DateTime.UtcNow;
                long timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                var product = new Products();
                product.ProductName = command.ProductName;
                product.ProductId = timestamp.ToString();
                product.Price = command.Price;
                product.CreatedAt = dateTime;
                product.OwnerID = command.OwnerID;
                await _productServices.Product_Create(product);

                return product;
            }
            public int CreateNumber()
            {
                Random random = new Random();
                int length = 5; // Độ dài chuỗi
                string numberString = "";

                for (int i = 0; i < length; i++)
                {
                    numberString += random.Next(0, 9).ToString(); // Tạo số ngẫu nhiên từ 0 đến 9
                }
                return int.Parse(numberString);
            }
        }
    }
}
    
