
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface ICommentRepository
    {
        Task<List<Comment>> DelListComment(int reviewID);
        Task<List<Comment>> GetListComment(int skip, int take, string str, long userID);
        Task<List<Comment>> GetCommentsByIdReview(int skip, int take, int reviewID);
        Task<Comment> CreateAComment(Comment comment);
    }
}
