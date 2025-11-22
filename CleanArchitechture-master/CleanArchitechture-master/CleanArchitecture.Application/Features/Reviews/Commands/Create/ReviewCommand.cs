using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Dtos;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Features.Reviews.Commands.Create
{

    public class ReviewCommand : IRequest<ReviewsDto>
    {
        public long OwnerID { get; set; }
        public double Rating { get; set; }
        public string? ReviewText { get; set; }
        public class CreateReviewCommandHandler : IRequestHandler<ReviewCommand, ReviewsDto>
        {
            private readonly IReviewServices _reviewServices;
            private readonly ILogger<CreateReviewCommandHandler> _logger;
            public CreateReviewCommandHandler(IReviewServices reviewServices, ILogger<CreateReviewCommandHandler> logger)
            {
                _reviewServices = reviewServices ?? throw new ArgumentNullException(nameof(reviewServices));
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }
            public async Task<ReviewsDto> Handle(ReviewCommand command, CancellationToken cancellationToken)
            {
                ReviewsDto Reviewdto = null;
                try
                {
                    _logger.LogInformation("CreateReviewCommand starting for owner {OwnerID}", command.OwnerID);
                    DateTime dateTime = DateTime.Now;
                    long timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                    var review = new Entites.Entites.Review();
                    review.OwnerID = command.OwnerID;
                    review.Rating = command.Rating;
                    review.ReviewText = command.ReviewText;
                    review.CreatedAt = dateTime;
                    Reviewdto = await _reviewServices.Review_Create(review, cancellationToken);
                    _logger.LogInformation("CreateReviewCommand completed for owner {OwnerID}", command.OwnerID);
                    return Reviewdto;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CreateReviewCommand failed for owner {OwnerID}", command.OwnerID);
                    return Reviewdto;
                }
            }
        }
    }
}

