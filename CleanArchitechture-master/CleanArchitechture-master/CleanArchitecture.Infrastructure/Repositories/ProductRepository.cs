using AutoMapper;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
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
        public ProductRepository(ApplicationContext userContext)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }
        public async Task<Product> CreateProduct(Product createProduct)
        {
            Product prod=null;
            try
            {
                await _userContext.AddAsync(createProduct);
                prod = createProduct;
                return prod;
            }
            catch
            {
            return prod;
            }

        }

        public async Task<List<Product>> GetListProducts(int skip, int take, string data)
        {
            List<Product>? products = null;
            try
            {
                             products = await _userContext.Products
                .Where(p => p.ProductName.StartsWith(data) || p.ProductName.EndsWith(data))
                .Skip(skip)
                .Take(Math.Min(take, 5000)) // Giới hạn số lượng bản ghi lấy tối đa
                .OrderBy(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
                if (products.Count() == 0)
                {
                    return null;
                }
                return products;
            }
            catch
            {
                return null;
            }
        }
        public async Task<Product> GetAProducts(int reviewId)
        {
            Product? products = null;
            try
            {
                             products = await _userContext.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ReviewID == reviewId);
                if (products == null)
                {
                    return null;
                }
                return products;
            }
            catch
            {
                return null;
            }
        }    
        public async Task<List<Product>> DelListProduct(int reviewId)
        {
            try
            {
                // Lấy danh sách các sản phẩm có ReviewID tương ứng
                var productsToDelete = await _userContext.Products
                    .Where(p => p.ReviewID == reviewId)
                    .ToListAsync();

                if (productsToDelete.Count == 0)
                {
                    // Có thể ném ra ngoại lệ hoặc trả về danh sách rỗng tùy theo yêu cầu của bạn
                    return null; // Hoặc throw new Exception("No products found");
                }

                // Xóa tất cả các sản phẩm trong danh sách
                _userContext.Products.RemoveRange(productsToDelete);

                return productsToDelete; // Trả về danh sách sản phẩm đã xóa
            }
            catch
            {
                return null;
            }
        }
        public async Task<Product> ProductUpdate(Product product)
        {
                Product? existingProduct = null;
            try
            {
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
                }
                // Chuyển đổi và trả về DTO
                return existingProduct;
            }
            catch
            {
                return null;
            }
        }
}
}
