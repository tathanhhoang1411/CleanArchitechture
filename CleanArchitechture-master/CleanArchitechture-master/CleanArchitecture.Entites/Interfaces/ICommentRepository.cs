
using CleanArchitecture.Entites.Entites;
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
