﻿using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Application.Query.Utilities;
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

    public class ProductCommand : IRequest<Products>
    {
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public long OwnerID { get; set; }
        public int ReviewID { get; set; }
        public class CreateProductCommandHandler : IRequestHandler<ProductCommand, Products>
        {
            private readonly IProductServices _productServices;
            public CreateProductCommandHandler(IProductServices productServices)
            {
                _productServices = productServices??throw new ArgumentNullException(nameof(productServices));
            }
            public async Task<Products> Handle(ProductCommand command, CancellationToken cancellationToken)
            {
                try
                {
                    DateTime dateTime = DateTime.Now;
                    long timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                    var product = new Products
                    {
                        ProductName = command.ProductName,
                        ProductId = timestamp.ToString(),
                        Price = command.Price,
                        CreatedAt = dateTime,
                        OwnerID = command.OwnerID,
                        ReviewID = command.ReviewID
                    };

                    await _productServices.Product_Create(product);
                    return product;
                }
                catch  
                {
                    return null;
                }
            }

        }
    }
}

