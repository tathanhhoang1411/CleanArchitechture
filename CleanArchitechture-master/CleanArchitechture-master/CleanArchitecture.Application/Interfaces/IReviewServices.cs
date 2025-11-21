using CleanArchitecture.Application.Utilities;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IReviewServices
    {
        Task<List<object>> GetList_Reviews(int skip, int take, string str, long userID);
        Task<ReviewsDto> Review_Create(Review review);
        Task<int> Review_Del(int reviewID, long userID);
        Task<List<Review>> GetList_Reviews_ByOwner(int reviewID, long userID);
    }
}
