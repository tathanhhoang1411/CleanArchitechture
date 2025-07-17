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

    public class DelUserCommand : IRequest<Users>
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public class DelUserCommandHandler : IRequestHandler<DelUserCommand, Users>
        {
            private readonly IUserServices _userServices;
            private readonly IMapper _mapper;
            public DelUserCommandHandler(IUserServices userServices, IUserRepository userRepository, IMapper mapper)
            {
                _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));
                _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

            }
            public async Task<Users> Handle(DelUserCommand command, CancellationToken cancellationToken)
            {
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
                    Boolean resultDelUser = await _userServices.DelUser(user);
                    return user;
                }
                catch
                {
                    return null ;
                }
            }

        }
    }
}

