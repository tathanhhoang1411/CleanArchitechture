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

    public class UpdStatusUserCommand : IRequest<UsersDto>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public class DelUserCommandHandler : IRequestHandler<UpdStatusUserCommand, UsersDto>
        {
            private readonly IUserServices _userServices;
            public DelUserCommandHandler(IUserServices userServices)
            {
                _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));

            }
            public async Task<UsersDto> Handle(UpdStatusUserCommand command, CancellationToken cancellationToken)
            {
                UsersDto resultDelUser = null;
                try
                {
                    Users user = new Users();
                    user.Username = command.Username;
                    user.Email = command.Email;
                    Users isExistUser = await _userServices.CheckExistUser(user);
                    if (isExistUser == null)//Nếu tài khoản cần xóa không tồn tại
                    {
                        return new UsersDto();
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

