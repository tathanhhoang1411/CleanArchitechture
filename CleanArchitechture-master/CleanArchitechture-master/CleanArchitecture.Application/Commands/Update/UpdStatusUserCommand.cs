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

namespace CleanArchitecture.Application.Commands.Update
{

    public class UpdStatusUserCommand : IRequest<UserDto>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public class UpdStatusUserCommandHandler : IRequestHandler<UpdStatusUserCommand, UserDto>
        {
            private readonly IUserServices _userServices;
            public UpdStatusUserCommandHandler(IUserServices userServices)
            {
                _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));

            }
            public async Task<UserDto> Handle(UpdStatusUserCommand command, CancellationToken cancellationToken)
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
                        return new UserDto();
                    }
                    resultDelUser = await _userServices.ActiveUser(user);
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

