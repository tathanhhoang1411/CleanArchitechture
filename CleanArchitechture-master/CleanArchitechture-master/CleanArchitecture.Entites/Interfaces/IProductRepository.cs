using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> CreateProduct(Product createProduct, CancellationToken cancellationToken = default);
        Task<List<Product>> GetListProducts(int skip, int take, string data, CancellationToken cancellationToken = default);
        Task<Product> GetAProducts(int reviewId, CancellationToken cancellationToken = default);
        Task<Product> ProductUpdate(Product product, CancellationToken cancellationToken = default);
        Task<List<Product>> DelListProduct(int reviewId, CancellationToken cancellationToken = default);
    }
}
