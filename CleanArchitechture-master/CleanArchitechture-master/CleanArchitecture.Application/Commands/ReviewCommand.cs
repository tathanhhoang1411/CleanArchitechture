using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Commands
{

    public class ReviewCommand : IRequest<Reviews>
    {
        public long OwnerID { get; set; }
        public string? ProductId { get; set; }
        public int Rating { get; set; }
        public string? ReviewText { get; set; }
        public class CreateReviewCommandHandler : IRequestHandler<ReviewCommand, Reviews>
        {
            private readonly IReviewServices _reviewServices;
            public CreateReviewCommandHandler(IReviewServices reviewServices)
            {
                _reviewServices = reviewServices;
            }
            public async Task<Reviews> Handle(ReviewCommand command, CancellationToken cancellationToken)
            {
                DateTime dateTime = DateTime.Now;
                long timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                var review = new Reviews();
                review.OwnerID = command.OwnerID;
                review.Rating = command.Rating;
                review.ReviewText = command.ReviewText;
                review.CreatedAt = dateTime;
                await _reviewServices.Review_Create(review);

                return review;
            }
        }
    }
}
    
