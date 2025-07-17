using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Query
{
    public class GetAllProductsQuery : IRequest<List<ProductDto>>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public string Data { get; set; }

        public GetAllProductsQuery(int skip, int take, string data)
        {
            Skip = skip;
            Take = take;
            Data = data;
        }
    }

    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, List<ProductDto>>
    {
        private readonly IProductServices _productServices;

        public GetAllProductsQueryHandler(IProductServices productServices)
        {
            _productServices = productServices;
        }

        public async Task<List<ProductDto>> Handle(GetAllProductsQuery query, CancellationToken cancellationToken)
        {
            // Lấy danh sách sản phẩm với phân trang
            var productList = await _productServices.GetList_Products(query.Skip, query.Take,query.Data);
            return productList ?? new List<ProductDto>(); // Trả về danh sách rỗng nếu không có sản phẩm
        }
    }
}