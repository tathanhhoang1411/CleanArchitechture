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

    public class UpdStatusUserCommand : IRequest<UsersDto>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public class UpdStatusUserCommandHandler : IRequestHandler<UpdStatusUserCommand, UsersDto>
        {
            private readonly IUserServices _userServices;
            public UpdStatusUserCommandHandler(IUserServices userServices)
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
                    if (isExistUser==null)//Nếu tài khoản cần xóa không tồn tại
                    {
                        return new UsersDto();
                    }
                    if (isExistUser.Status == true)//Nếu tài khoản đang được kích hoạt
                    {
                    resultDelUser = await _userServices.DelUser(user);

                    }
                    else//Nếu tài khoản đang được tắt dùng
                    {

                    resultDelUser = await _userServices.ActiveUser(user);
                    }
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

