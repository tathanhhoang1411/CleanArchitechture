
using CleanArchitecture.Application.Interfaces;
using MediatR;

namespace CleanArchitecture.Application.Features.Reviews.Commands.Delete
{

    public class DelReviewCommand : IRequest<int>
    {
        public int ReviewID { get; set; }
        public long UserID { get; set; }
        public class DelReviewCommandHandler : IRequestHandler<DelReviewCommand, int>
        {
            private readonly IReviewServices _reviewServices;
            public DelReviewCommandHandler(IReviewServices reviewServices)
            {
                _reviewServices = reviewServices ?? throw new ArgumentNullException(nameof(reviewServices));

            }
            public async Task<int> Handle(DelReviewCommand command, CancellationToken cancellationToken)
            {
                int reviewID = -1;
                try
                {
                    reviewID = await _reviewServices.Review_Del(command.ReviewID,command.UserID);
                    if (reviewID == 0)
                    {
                        return -1;
                    }
                    return reviewID;
                }
                catch
                {
                    return -1;
                }
            }

        }
    }
}

