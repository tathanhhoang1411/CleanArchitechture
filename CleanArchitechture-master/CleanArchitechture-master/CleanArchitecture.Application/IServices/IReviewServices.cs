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
        Task<List<ReviewDto>> GetList_Reviews(int skip, int take,string str);
        Task<int> Review_Create(Reviews review);
    }
}
