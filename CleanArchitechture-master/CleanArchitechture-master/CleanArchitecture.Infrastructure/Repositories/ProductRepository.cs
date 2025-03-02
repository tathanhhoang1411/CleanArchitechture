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

        public async Task<int> CreateProduct(Products createProduct)
        {
            _userContext.Add(createProduct);
            return await  _userContext.SaveChangesAsync();
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



            //return await _userContext.Products
            // .Select(p => new Produ
            // {

            //     ProductId = p.ProductId, // Có thể bỏ qua nếu không cần
            //     ProductName = p.ProductName,
            //     ProductImage1 = p.ProductImage1,
            //     ProductImage2 = p.ProductImage2,
            //     ProductImage3 = p.ProductImage3,
            //     ProductImage4 = p.ProductImage4,
            //     ProductImage5 = p.ProductImage5,
            //     Price = p.Price
            // })
            // .ToListAsync();
        }
    }
}
