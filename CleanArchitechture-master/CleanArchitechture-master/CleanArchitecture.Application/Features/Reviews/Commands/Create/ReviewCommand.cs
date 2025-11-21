using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Repository;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            public CreateReviewCommandHandler(IReviewServices reviewServices)
            {
                _reviewServices = reviewServices ?? throw new ArgumentNullException(nameof(reviewServices));
            }
            public async Task<ReviewsDto> Handle(ReviewCommand command, CancellationToken cancellationToken)
            {
                    ReviewsDto Reviewdto=null;
                try
                {
                    DateTime dateTime = DateTime.Now;
                    long timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                    var review = new Entites.Entites.Review();
                    review.OwnerID = command.OwnerID;
                    review.Rating = command.Rating;
                    review.ReviewText = command.ReviewText;
                    review.CreatedAt = dateTime;
                    Reviewdto=await _reviewServices.Review_Create(review);
                    return Reviewdto;
                }
                catch
                {
                    return Reviewdto;
                }
            }
        }
    }
}

