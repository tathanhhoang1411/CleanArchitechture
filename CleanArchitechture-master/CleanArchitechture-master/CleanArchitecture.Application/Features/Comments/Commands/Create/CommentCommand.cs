using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using MediatR;
using CleanArchitecture.Application.Interfaces;
using System.Threading;

namespace CleanArchitecture.Application.Features.Comments.Commands.Create
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
                    Entites.Entites.Comment comment = new Entites.Entites.Comment();
                    comment.CommentId = timestamp;
                    comment.CommentText = command.ReviewText;
                    comment.ReviewId = command.ReviewID;
                    comment.CreatedAt = dateTime;
                    comment.UserId = command.OwnerID;
                    commentDto = await _commentServices.Comment_Create(comment, cancellationToken);
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

