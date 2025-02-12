using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Query
{
    public class GetAllProductsQuery : IRequest<List<ProductDto>>
    {

        public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
        {

            private readonly IProductRepository _productRepository;
            public GetAllProductsQueryHandler(IProductRepository productRepository)
            {
                _productRepository = productRepository;
            }
            public async Task<List<ProductDto>> Handle(GetAllProductsQuery query, CancellationToken cancellationToken)
            {
                var productList = await _productRepository.GetListProduct();
                return productList ?? new List<ProductDto>(); // Trả về danh sách rỗng nếu không có sản phẩm
            }
        }
    }
}
