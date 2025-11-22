
using CleanArchitecture.Entites.Entites;
namespace CleanArchitecture.Entites.Interfaces
{
    public interface IReviewRepository
    {
        Task<Review> CreateReview(Review createReview);
        Task<int> DelReview(int reviewID);
        Task<List<Review>> GetListReviewsByOwnerID(int reviewID, long ownerID); 
        Task<List<object>> GetListReviews(int skip, int take, string str , long userID);
    }
}
