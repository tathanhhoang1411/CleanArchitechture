using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ApplicationContext _userContext;
        private readonly int _maxTake;

        public ProductRepository(ApplicationContext userContext, IConfiguration configuration)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            var configured = configuration.GetValue<int?>("Paging:MaxTake");
            _maxTake = (configured.HasValue && configured.Value > 0) ? configured.Value : 5000;
        }

        public async Task<Product> CreateProduct(Product createProduct, CancellationToken cancellationToken = default)
        {
            if (createProduct == null) return null;

            try
            {
                if (createProduct.CreatedAt == default)
                    createProduct.CreatedAt = DateTime.UtcNow;

                await _userContext.Products.AddAsync(createProduct, cancellationToken);
                return createProduct;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Product>> GetListProducts(int skip, int take, string data, CancellationToken cancellationToken = default)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 10;
            take = Math.Min(take, _maxTake);

            try
            {
                var query = _userContext.Products.AsQueryable();

                if (!string.IsNullOrWhiteSpace(data))
                {
                    query = query.Where(p => p.ProductName != null && (p.ProductName.StartsWith(data) || p.ProductName.EndsWith(data)));
                }

                var products = await query
                    .OrderBy(p => p.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                return products ?? new List<Product>();
            }
            catch
            {
                return new List<Product>();
            }
        }

        public async Task<Product> GetAProducts(int reviewId, CancellationToken cancellationToken = default)
        {
            try
            {
                var product = await _userContext.Products
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.ReviewID == reviewId, cancellationToken);
                return product;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Product>> DelListProduct(int reviewId, CancellationToken cancellationToken = default)
        {
            if (reviewId <= 0) return new List<Product>();

            try
            {
                var productsToDelete = await _userContext.Products
                    .Where(p => p.ReviewID == reviewId)
                    .ToListAsync(cancellationToken);

                if (productsToDelete == null || productsToDelete.Count == 0)
                    return new List<Product>();

                _userContext.Products.RemoveRange(productsToDelete);
                return productsToDelete;
            }
            catch
            {
                return new List<Product>();
            }
        }

        public async Task<Product> ProductUpdate(Product product, CancellationToken cancellationToken = default)
        {
            if (product == null) return null;

            try
            {
                var existingProduct = await _userContext.Products.FirstOrDefaultAsync(p => p.ProductId == product.ProductId, cancellationToken);
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

                    _userContext.Products.Update(existingProduct);
                }

                return existingProduct;
            }
            catch
            {
                return null;
            }
        }
    }
}
