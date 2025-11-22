using CleanArchitecture.Application.Interfaces;
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

namespace CleanArchitecture.Application.Features.Comments.Query
{

    public class CommentQuerySelect : IRequest<List<CommentsDto>>
    {
        public int Skip { get; set; }
        public int Take { get; set; }
        public QueryEF Data { get; set; }

        public CommentQuerySelect(int skip, int take, QueryEF data)
        {
            Skip = skip;
            Take = take;
            Data = data;
        }
        public int ReviewID { get; set; }
        public class CommentQuerySelectHandler : IRequestHandler<CommentQuerySelect, List<CommentsDto>>
        {
            private readonly ICommentServices _commetServices;
            public CommentQuerySelectHandler(ICommentServices commentServices)
            {
                _commetServices = commentServices ?? throw new ArgumentNullException(nameof(commentServices));
            }

            public async Task<List<CommentsDto>> Handle(CommentQuerySelect query, CancellationToken cancellationToken)
            {
                try
                {
                    var CommentDtoList = await _commetServices.GetList_Comment_ByOwner(query.Skip, query.Take, query.Data.str, query.Data.ID, cancellationToken);

                    if (CommentDtoList != null)
                        return CommentDtoList;
                    else
                        return new List<CommentsDto>(); // Trả về danh sách rỗng nếu không có bài review
                }
                catch
                {
                    return null;
                }
            }
        }
    }
}

