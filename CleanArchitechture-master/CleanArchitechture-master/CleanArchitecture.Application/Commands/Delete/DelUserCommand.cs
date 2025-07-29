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

    public class DelUserCommand : IRequest<UserDto>
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public class DelUserCommandHandler : IRequestHandler<DelUserCommand, UserDto>
        {
            private readonly IUserServices _userServices;
            private readonly IMapper _mapper;
            public DelUserCommandHandler(IUserServices userServices, IUserRepository userRepository, IMapper mapper)
            {
                _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            }
            public async Task<UserDto> Handle(DelUserCommand command, CancellationToken cancellationToken)
            {
                UserDto resultDelUser = null;
                try
                {
                    Users user = new Users();
                    user.Username = command.Username;
                    user.Email = command.Email;
                    bool isExistUser = await _userServices.CheckExistUser(user);
                    if (!isExistUser)//Nếu tài khoản cần xóa không tồn tại
                    {
                        return null;
                    }
                    resultDelUser = await _userServices.DelUser(user);
                    return resultDelUser;
                }
                catch
                {
                    return resultDelUser;
                }
            }

        }
    }
}

