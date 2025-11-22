
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
namespace CleanArchitecture.Application.Interfaces
{
    public interface ICommentServices
    {

        Task<List<CommentsDto>> GetList_Comment_ByOwner(int skip, int take, string str, long userID);
        Task<List<CommentsDto>> GetList_Comment_ByReviewID(int skip, int take, int reviewID);
        Task<CommentsDto> Comment_Create(Comment comments);
    }
}
