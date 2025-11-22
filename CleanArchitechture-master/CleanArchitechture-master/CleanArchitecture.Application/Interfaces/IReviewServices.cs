using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IReviewServices
    {
        Task<List<object>> GetList_Reviews(int skip, int take, string str, long userID, CancellationToken cancellationToken = default);
        Task<ReviewsDto> Review_Create(Review review, CancellationToken cancellationToken = default);
        Task<int> Review_Del(int reviewID, long userID, CancellationToken cancellationToken = default);
        Task<List<Review>> GetList_Reviews_ByOwner(int reviewID, long userID, CancellationToken cancellationToken = default);
    }
}
