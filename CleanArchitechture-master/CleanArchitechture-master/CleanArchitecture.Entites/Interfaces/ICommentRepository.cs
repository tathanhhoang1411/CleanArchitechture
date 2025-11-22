using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Entites.Entites;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface ICommentRepository
    {
        Task<List<Comment>> DelListComment(int reviewID, CancellationToken cancellationToken = default);
        Task<List<Comment>> GetListComment(int skip, int take, string str, long userID, CancellationToken cancellationToken = default);
        Task<List<Comment>> GetCommentsByIdReview(int skip, int take, int reviewID, CancellationToken cancellationToken = default);
        Task<Comment> CreateAComment(Comment comment, CancellationToken cancellationToken = default);
    }
}
