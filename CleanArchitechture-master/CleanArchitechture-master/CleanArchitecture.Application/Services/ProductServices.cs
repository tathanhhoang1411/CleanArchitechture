using AutoMapper;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Repository
{
    public class ProductServices : IProductServices
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly int _maxTake = 5000;

        public ProductServices(IProductRepository productRepository, IUnitOfWork unitOfWork,IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<List<ProductsDto>> GetList_Products(int skip, int take, string data, CancellationToken cancellationToken = default)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 10;
            take = Math.Min(take, _maxTake);

            try
            {
                var listProduct = await _unitOfWork.Products.GetListProducts(skip, take, data, cancellationToken);
                return _mapper.Map<List<ProductsDto>>(listProduct ?? new List<Product>());
            }
            catch
            {
                return new List<ProductsDto>();
            }
        }

        public async Task<ProductsDto> Product_Create(Product product, CancellationToken cancellationToken = default)
        {
            if (product == null) return null;

            try
            {
                if (product.CreatedAt == default)
                    product.CreatedAt = DateTime.UtcNow;

                await _unitOfWork.Products.CreateProduct(product, cancellationToken);
                await _unitOfWork.CompleteAsync(cancellationToken);
                return _mapper.Map<ProductsDto>(product);

            }
            catch
            {
                return null;
            }
        }
        public async Task<ProductsDto> GetA_Products(int reviewId, CancellationToken cancellationToken = default)
        {
            try
            {
                var prod = await _unitOfWork.Products.GetAProducts(reviewId, cancellationToken);
                return _mapper.Map<ProductsDto>(prod);
            }
            catch
            {
                return null;
            }
        }
        public async Task<ProductsDto> Product_Update(Product product, CancellationToken cancellationToken = default)
        {
            if (product == null) return null;

            try
            {
                var prod = await _unitOfWork.Products.ProductUpdate(product, cancellationToken);
                await _unitOfWork.CompleteAsync(cancellationToken);
                return _mapper.Map<ProductsDto>(prod);
            }
            catch
            {
                return null;
            }
        }
    }
}
