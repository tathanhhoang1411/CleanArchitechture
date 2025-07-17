using AutoMapper;
using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Query.Utilities
{
    public class ValidateToken : IRequest<string>
    {
        public string AccessToken { get; set; }

        public class ValidateTokenHandler : IRequestHandler<ValidateToken, string>
        {
            private readonly IUserRepository _userRepository;
            private readonly IUserServices _userServices;

            public ValidateTokenHandler(IUserRepository userRepository, IUserServices userServices)
            {
                _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
                _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
            }

            public async Task<string> Handle(ValidateToken request, CancellationToken cancellationToken)
            {
                string validateToken = request.AccessToken;




                // Kiểm tra nếu token rỗng hoặc null
                if (string.IsNullOrEmpty(validateToken))
                {
                    return null;
                }

                // Gọi phương thức ValidateToken từ IUserServices
                ClaimsPrincipal check = _userServices.ValidateToken(validateToken);
                if (check == null)
                {
                    return null;
                }
                return "Success";
            }
        }
    }
}