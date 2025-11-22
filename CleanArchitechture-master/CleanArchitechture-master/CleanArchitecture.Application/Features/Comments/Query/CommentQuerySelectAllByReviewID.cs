using CleanArchitecture.Application.Dtos;
using MediatR;
using CleanArchitecture.Application.Interfaces;
using System.Threading;

namespace CleanArchitecture.Application.Features.Comments.Query
{

    public class CommentQuerySelectAllByReviewID : IRequest<List<CommentsDto>>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public int ReviewID { get; set; }

        public CommentQuerySelectAllByReviewID(int skip, int take, int reviewID)
        {
            Skip = skip;
            Take = take;
            ReviewID = reviewID;
        }
        public class CommentQuerySelectHandler : IRequestHandler<CommentQuerySelectAllByReviewID, List<CommentsDto>>
        {
            private readonly ICommentServices _commetServices;
            public CommentQuerySelectHandler(ICommentServices commentServices)
            {
                _commetServices = commentServices ?? throw new ArgumentNullException(nameof(commentServices));
            }

            public async Task<List<CommentsDto>> Handle(CommentQuerySelectAllByReviewID query, CancellationToken cancellationToken)
            {
                try
                {
                    var CommentDtoList = await _commetServices.GetList_Comment_ByReviewID(query.Skip, query.Take, query.ReviewID, cancellationToken);

                    return CommentDtoList ?? new List<CommentsDto>(); // Trả về danh sách rỗng nếu không có bài review
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}

