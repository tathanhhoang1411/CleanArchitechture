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

    public class ReviewCommand : IRequest<ReviewDto>
    {
        public long OwnerID { get; set; }
        public double Rating { get; set; }
        public string? ReviewText { get; set; }
        public class CreateReviewCommandHandler : IRequestHandler<ReviewCommand, ReviewDto>
        {
            private readonly IReviewServices _reviewServices;
            public CreateReviewCommandHandler(IReviewServices reviewServices)
            {
                _reviewServices = reviewServices ?? throw new ArgumentNullException(nameof(reviewServices));
            }
            public async Task<ReviewDto> Handle(ReviewCommand command, CancellationToken cancellationToken)
            {
                    ReviewDto Reviewdto=null;
                try
                {
                    DateTime dateTime = DateTime.Now;
                    long timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                    var review = new Reviews();
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

