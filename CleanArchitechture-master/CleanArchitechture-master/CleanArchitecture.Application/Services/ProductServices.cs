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
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public ProductServices(IProductRepository productRepository, IUnitOfWork unitOfWork,IMapper mapper)
        {
            _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<List<ProductDto>> GetList_Products(int skip, int take, string data)
        {
            List<Products> listProduct   =await _productRepository.GetListProducts(skip, take, data);
            return  _mapper.Map<List<ProductDto>>(listProduct);
        }

        public async Task<ProductDto> Product_Create(Products product)
        {
            try
            {
                await _unitOfWork.Products.CreateProduct(product);
                await _unitOfWork.CompleteAsync();
                return _mapper.Map<ProductDto>(product);

            }
            catch
            {
                return new ProductDto();
            }
        }
        public async Task<ProductDto> GetA_Products(int reviewId)
        {
            Products prod=await _productRepository.GetAProducts(reviewId);
            return _mapper.Map<ProductDto>(prod);
        }
        public async Task<ProductDto> Product_Update(Products product)
        {
            Products prod = await _productRepository.ProductUpdate(product);
            return _mapper.Map<ProductDto>(prod);
        }
    }
}
