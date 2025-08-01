using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public interface IReviewRepository
    {
        Task<Reviews> CreateReview(Reviews createReview);
        Task<int> DelReview(int reviewID);
        Task<List<Reviews>> GetListReviewsByOwnerID(int reviewID, long ownerID); 
        Task<List<object>> GetListReviews(int skip, int take, string str , long userID);
    }
}
