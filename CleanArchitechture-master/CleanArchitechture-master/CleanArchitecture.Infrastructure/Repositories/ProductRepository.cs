using AutoMapper;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace CleanArchitecture.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private ApplicationContext _userContext;
        private readonly IMapper _mapper;
        public ProductRepository(ApplicationContext userContext, IMapper mapper)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<int> CreateProduct(Products createProduct)
        {
            _userContext.Add(createProduct);
            return await  _userContext.SaveChangesAsync();
        }

        public async Task<List<ProductDto>> GetListProducts(int skip, int take, string data)
        {
            var products = await _userContext.Products
                .Where(p => p.ProductName.Contains(data))
                .Take(take)
                .Skip(skip)
                .OrderBy(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
            return _mapper.Map<List<ProductDto>>(products);
        }
        public async Task<ProductDto> GetAProducts(int reviewId)
        {
            var product = await _userContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ReviewID == reviewId);

            return _mapper.Map<ProductDto>(product);
        }
        public async Task<ProductDto> ProductUpdate(Products product)
        {
            // Tìm sản phẩm trong cơ sở dữ liệu
            var existingProduct = await _userContext.Products.FindAsync(product.ProductId);
            if (existingProduct == null)
            {
                throw new Exception("Product not found.");
            }
            // Cập nhật các thuộc tính của sản phẩm
            existingProduct.ProductName = product.ProductName;
    existingProduct.ProductImage1 = product.ProductImage1;
    existingProduct.ProductImage2 = product.ProductImage2;
    existingProduct.ProductImage3 = product.ProductImage3;
    existingProduct.ProductImage4 = product.ProductImage4;
    existingProduct.ProductImage5 = product.ProductImage5;
    existingProduct.Price = product.Price;
    existingProduct.OwnerID = product.OwnerID;
    existingProduct.ReviewID = product.ReviewID;
    existingProduct.ProductId = product.ProductId;

    // Lưu thay đổi vào cơ sở dữ liệu
    await _userContext.SaveChangesAsync();

    // Chuyển đổi và trả về DTO
    return _mapper.Map<ProductDto>(existingProduct);
        }
}
}
