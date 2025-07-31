using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public interface IProductRepository
    {
        Task<Products> CreateProduct(Products createProduct);
        Task<List<Products>> GetListProducts(int skip, int take,string data);
        Task<Products> GetAProducts(int reviewId);
        Task<Products> ProductUpdate(Products product);
        Task<List<Products>> DelListProduct(int reviewId);
    }
}
