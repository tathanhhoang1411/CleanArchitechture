using CleanArchitecture.Application.Query.Utilities;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.IRepository
{
    public interface IReviewServices
    {
        Task<List<object>> GetList_Reviews(int skip, int take, string str, long userID);
        Task<ReviewDto> Review_Create(Reviews review);
        Task<int> Review_Del(int reviewID, long userID);
        Task<List<Reviews>> GetList_Reviews_ByOwner(int reviewID, long userID);
    }
}
