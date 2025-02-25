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

    public class CreateProductCommand : IRequest<Products>
    {
        public string ProductName { get; set; }
        public string? ProductImg1 { get; set; }
        public string? ProductImg2 { get; set; }
        public string? ProductImg3 { get; set; }
        public string? ProductImg4 { get; set; }
        public string? ProductImg5 { get; set; }
        public decimal Price { get; set; }
        public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Products>
        {
            private readonly IProductRepository _productRepository;
            public CreateProductCommandHandler(IProductRepository productRepository)
            {
                _productRepository = productRepository;
            }
            public async Task<Products> Handle(CreateProductCommand command, CancellationToken cancellationToken)
            {
                DateTime dateTime = DateTime.UtcNow;
                long timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                var product = new Products();
                product.ProductName = command.ProductName;
                product.ProductImage1 = command.ProductImg1;
                product.ProductImage2 = command.ProductImg2;
                product.ProductImage3 = command.ProductImg3;
                product.ProductImage4 = command.ProductImg4;
                product.ProductImage5 = command.ProductImg5;
                product.ProductId = timestamp.ToString();
                product.Price = command.Price;
                product.CreatedAt = dateTime;
                await _productRepository.CreateProduct(product);

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
    
