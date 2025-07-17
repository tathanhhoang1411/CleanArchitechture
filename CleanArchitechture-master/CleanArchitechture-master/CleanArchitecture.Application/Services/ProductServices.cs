using AutoMapper;
using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.DBContext;
using CleanArchitecture.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Repository
{
    public class ProductServices : IProductServices
    {
        private readonly IProductRepository _productRepository;
        public ProductServices(IProductRepository productRepository, IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        }

        public Task<List<ProductDto>> GetList_Products(int skip, int take, string data)
        {
            return _productRepository.GetListProducts(skip,take, data);
        }

        public async Task Product_Create(Products product)
        {
            await _productRepository.CreateProduct(product);
            return ;

        }
        public Task<ProductDto> GetA_Products(int reviewId)
        {
            return _productRepository.GetAProducts(reviewId);
        }
        public Task<ProductDto> Product_Update(Products product)
        {
             var result=_productRepository.ProductUpdate(product);
            return result;
        }
    }
}
