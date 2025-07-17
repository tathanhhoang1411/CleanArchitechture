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
        public async Task CreateProduct(Products createProduct)
        {
             await _userContext.AddAsync(createProduct);
            await _userContext.SaveChangesAsync();
            return ;
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
            var existingProduct = new Products();
            // Tìm sản phẩm trong cơ sở dữ liệu
             existingProduct = await _userContext.Products.FirstOrDefaultAsync(p => p.ProductId == product.ProductId);
  if (existingProduct != null)
    {
        existingProduct.ProductName = product.ProductName;
        existingProduct.ProductImage1 = product.ProductImage1;
        existingProduct.ProductImage2 = product.ProductImage2;
        existingProduct.ProductImage3 = product.ProductImage3;
        existingProduct.ProductImage4 = product.ProductImage4;
        existingProduct.ProductImage5 = product.ProductImage5;
        existingProduct.Price = product.Price;
        existingProduct.OwnerID = product.OwnerID;
        existingProduct.ReviewID = product.ReviewID;

        // Cập nhật đối tượng trong ngữ cảnh
        _userContext.Products.Update(existingProduct);
        await _userContext.SaveChangesAsync();
    }
            // Chuyển đổi và trả về DTO
            return _mapper.Map<ProductDto>(existingProduct);
        }
}
}
