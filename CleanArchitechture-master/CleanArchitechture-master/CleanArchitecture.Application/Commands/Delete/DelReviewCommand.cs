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

    public class DelReviewCommand : IRequest<ReviewDto>
    {
        public long ReviewID { get; set; }
        public long UserID { get; set; }
        public class DelReviewCommandHandler : IRequestHandler<DelReviewCommand, ReviewDto>
        {
            private readonly IUserServices _userServices;
            public DelReviewCommandHandler(IUserServices userServices, IUserRepository userRepository, IMapper mapper)
            {
                _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));

            }
            public async Task<ReviewDto> Handle(DelReviewCommand command, CancellationToken cancellationToken)
            {
                ReviewDto DelreviewDto = null;
                try
                {

                    return DelreviewDto;
                }
                catch
                {
                    return DelreviewDto;
                }
            }

        }
    }
}

