using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public interface ICommentRepository
    {
        Task<List<Comments>> DelListComment(int reviewID);
        Task<List<Comments>> GetListComment(int skip, int take, string str, long userID);
        Task<List<Comments>> GetCommentsByIdReview(int skip, int take, int reviewID);
        Task<Comments> CreateAComment(Comments comment);
    }
}
