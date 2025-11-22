using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using System.Threading;

namespace CleanArchitecture.Application.Interfaces
{
    public interface ICommentServices
    {

        Task<List<CommentsDto>> GetList_Comment_ByOwner(int skip, int take, string str, long userID, CancellationToken cancellationToken = default);
        Task<List<CommentsDto>> GetList_Comment_ByReviewID(int skip, int take, int reviewID, CancellationToken cancellationToken = default);
        Task<CommentsDto> Comment_Create(Comment comments, CancellationToken cancellationToken = default);
    }
}
