
using CleanArchitecture.Application.Utilities;
using CleanArchitecture.Application.Repository;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CleanArchitecture.Application.Interfaces;

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
                    var CommentDtoList = await _commetServices.GetList_Comment_ByReviewID(query.Skip, query.Take, query.ReviewID);

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

