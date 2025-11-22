using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using MediatR;
using CleanArchitecture.Application.Interfaces;
using System.Threading;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

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
            private readonly ILogger<CreateCommentCommandHandler> _logger;
            public CreateCommentCommandHandler(ICommentServices commentServices, ILogger<CreateCommentCommandHandler> logger)
            {
                _commentServices = commentServices ?? throw new ArgumentNullException(nameof(commentServices));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }
            public async Task<CommentsDto> Handle(CommentCommand command, CancellationToken cancellationToken)
            {
                CommentsDto commentDto = null;
                try
                {
                    _logger.LogInformation("CreateCommentCommand starting for user {UserId}", command.OwnerID);
                    DateTime dateTime = DateTime.Now;
                    long timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                    Entites.Entites.Comment comment = new Entites.Entites.Comment();
                    comment.CommentId = timestamp;
                    comment.CommentText = command.ReviewText;
                    comment.ReviewId = command.ReviewID;
                    comment.CreatedAt = dateTime;
                    comment.UserId = command.OwnerID;
                    commentDto = await _commentServices.Comment_Create(comment, cancellationToken);
                    _logger.LogInformation("CreateCommentCommand completed for comment {CommentId}", comment.CommentId);
                    return commentDto;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CreateCommentCommand failed for user {UserId}", command.OwnerID);
                    return commentDto;
                }
            }
        }
    }
}

