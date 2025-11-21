using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IProductServices
    {
        Task<List<ProductsDto>> GetList_Products(int skip, int take,string str);
        Task<ProductsDto> Product_Create(Product product);
        Task<ProductsDto> GetA_Products(int reviewId);
        Task<ProductsDto> Product_Update(Product product);
    }
}
