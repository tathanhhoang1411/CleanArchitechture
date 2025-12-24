using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Interfaces;
using MediatR;

namespace CleanArchitecture.Application.Features.Users.Query
{
    public class GetUserWithDetailQuery : IRequest<UserWithDetailDto>
    {
        public string Identifier { get; }
        public GetUserWithDetailQuery(string identifier)
        {
            Identifier = identifier;
        }

        public class Handler : IRequestHandler<GetUserWithDetailQuery, UserWithDetailDto>
        {
            private readonly IUserServices _userServices;
            public Handler(IUserServices userServices)
            {
                _userServices = userServices;
            }

            public async Task<UserWithDetailDto> Handle(GetUserWithDetailQuery request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(request.Identifier)) return null;
                return await _userServices.GetUserWithDetailByIdentifier(request.Identifier);
            }
        }
    }
}