using CleanArchitecture.Application.IRepository;
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

namespace CleanArchitecture.Application.Commands.Create
{

    public class CommentCommand : IRequest<CommentsDto>
    {
        public long OwnerID { get; set; }
        public int ReviewID { get; set; }
        public string? ReviewText { get; set; }
        public class CreateCommentCommandHandler : IRequestHandler<CommentCommand, CommentsDto>
        {
            private readonly ICommentServices _commentServices;
            public CreateCommentCommandHandler(ICommentServices commentServices)
            {
                _commentServices = commentServices ?? throw new ArgumentNullException(nameof(commentServices));
            }
            public async Task<CommentsDto> Handle(CommentCommand command, CancellationToken cancellationToken)
            {
                    CommentsDto commentDto=null;
                try
                {
                    DateTime dateTime = DateTime.Now;
                    long timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                    Comments comment = new Comments();
                    comment.CommentId = timestamp;
                    comment.CommentText = command.ReviewText;
                    comment.ReviewId = command.ReviewID;
                    comment.CreatedAt = dateTime;
                    comment.UserId = command.OwnerID;
                    commentDto = await _commentServices.Comment_Create(comment);
                    return commentDto;
                }
                catch
                {
                    return commentDto;
                }
            }
        }
    }
}

