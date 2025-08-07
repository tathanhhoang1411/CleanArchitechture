using AutoMapper;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CleanArchitecture.Application.Commands.Delete
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

