using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.IRepository
{
    public interface IProductServices
    {
        Task<List<ProductsDto>> GetList_Products(int skip, int take,string str);
        Task<ProductsDto> Product_Create(Products product);
        Task<ProductsDto> GetA_Products(int reviewId);
        Task<ProductsDto> Product_Update(Products product);
    }
}
