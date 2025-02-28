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
        Task<List<ProductDto>> GetList_Products(int skip, int take,string str);
        Task<int> Product_InsertUpdate(Products product);
    }
}
