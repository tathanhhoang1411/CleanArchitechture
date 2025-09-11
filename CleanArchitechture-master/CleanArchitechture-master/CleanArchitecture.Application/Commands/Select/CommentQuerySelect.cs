using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Application.Query.Utilities;
using CleanArchitecture.Application.Repository;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Commands.Select
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
                    var CommentDtoList = await _commetServices.GetList_Comment_ByOwner(query.Skip, query.Take, query.Data.str, query.Data.ID);

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

