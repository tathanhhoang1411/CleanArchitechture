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
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public ProductServices(IProductRepository productRepository, IUnitOfWork unitOfWork,IMapper mapper)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<List<ProductDto>> GetList_Products(int skip, int take, string data)
        {
            List<ProductDto> listProductdto=null;
            try
            {
                List<Products> listProduct = await _unitOfWork.Products.GetListProducts(skip, take, data);
                return _mapper.Map<List<ProductDto>>(listProduct);
            }
            catch
            {
                return listProductdto;
            }
        }

        public async Task<ProductDto> Product_Create(Products product)
        {
            ProductDto dto = null;
            try
            {
                await _unitOfWork.Products.CreateProduct(product);
                await _unitOfWork.CompleteAsync();
                return _mapper.Map<ProductDto>(product);

            } 
            catch
            {
                return dto;
            }
        }
        public async Task<ProductDto> GetA_Products(int reviewId)
        {
            ProductDto dto = null;
            try
            {
                Products prod = await _unitOfWork.Products.GetAProducts(reviewId);
                return _mapper.Map<ProductDto>(prod);
            }
            catch
            {
                return dto;
            }
        }
        public async Task<ProductDto> Product_Update(Products product)
        {
            ProductDto dto = null;
            try
            {
                Products prod = await _unitOfWork.Products.ProductUpdate(product);
                await _unitOfWork.CompleteAsync();
                return _mapper.Map<ProductDto>(prod);
            }catch
            {
                return dto;
            }
        }
    }
}
