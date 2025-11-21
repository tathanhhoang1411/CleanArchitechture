
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> CreateProduct(Product createProduct);
        Task<List<Product>> GetListProducts(int skip, int take,string data);
        Task<Product> GetAProducts(int reviewId);
        Task<Product> ProductUpdate(Product product);
        Task<List<Product>> DelListProduct(int reviewId);
    }
}
