using CleanArchitecture.Entites.Entites;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IReviewRepository
    {
        Task<Review> CreateReview(Review createReview, CancellationToken cancellationToken = default);
        Task<int> DelReview(int reviewID, CancellationToken cancellationToken = default);
        Task<List<Review>> GetListReviewsByOwnerID(int reviewID, long ownerID, CancellationToken cancellationToken = default);
        Task<List<object>> GetListReviews(int skip, int take, string str , long userID, CancellationToken cancellationToken = default);
    }
}
