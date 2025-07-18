﻿using CleanArchitecture.Entites.Dtos;
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
        Task CreateProduct(Products createProduct);
        Task<List<ProductDto>> GetListProducts(int skip, int take,string data);
        Task<ProductDto> GetAProducts(int reviewId);
        Task<ProductDto> ProductUpdate(Products product);
    }
}
