using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IProductServices
    {
        Task<List<ProductsDto>> GetList_Products(int skip, int take, string str, CancellationToken cancellationToken = default);
        Task<ProductsDto> Product_Create(Product product, CancellationToken cancellationToken = default);
        Task<ProductsDto> GetA_Products(int reviewId, CancellationToken cancellationToken = default);
        Task<ProductsDto> Product_Update(Product product, CancellationToken cancellationToken = default);
    }
}
