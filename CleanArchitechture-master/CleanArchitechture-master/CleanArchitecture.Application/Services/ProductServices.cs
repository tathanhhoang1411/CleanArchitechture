using AutoMapper;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Entites.Interfaces;

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

        public async Task<List<ProductsDto>> GetList_Products(int skip, int take, string data)
        {
            List<ProductsDto> listProductdto=null;
            try
            {
                List<Product> listProduct = await _unitOfWork.Products.GetListProducts(skip, take, data);
                return _mapper.Map<List<ProductsDto>>(listProduct);
            }
            catch
            {
                return listProductdto;
            }
        }

        public async Task<ProductsDto> Product_Create(Product product)
        {
            ProductsDto dto = null;
            try
            {
                await _unitOfWork.Products.CreateProduct(product);
                await _unitOfWork.CompleteAsync();
                return _mapper.Map<ProductsDto>(product);

            } 
            catch
            {
                return dto;
            }
        }
        public async Task<ProductsDto> GetA_Products(int reviewId)
        {
            ProductsDto dto = null;
            try
            {
                Product prod = await _unitOfWork.Products.GetAProducts(reviewId);
                return _mapper.Map<ProductsDto>(prod);
            }
            catch
            {
                return dto;
            }
        }
        public async Task<ProductsDto> Product_Update(Product product)
        {
            ProductsDto dto = null;
            try
            {
                Product prod = await _unitOfWork.Products.ProductUpdate(product);
                await _unitOfWork.CompleteAsync();
                return _mapper.Map<ProductsDto>(prod);
            }catch
            {
                return dto;
            }
        }
    }
}
