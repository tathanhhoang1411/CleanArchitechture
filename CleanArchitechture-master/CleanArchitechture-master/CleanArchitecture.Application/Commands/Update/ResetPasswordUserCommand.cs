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

    public class ResetPasswordUserCommand : IRequest<UsersDto>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public class ResetPasswordUserCommandHandler : IRequestHandler<ResetPasswordUserCommand, UsersDto>
        {
            private readonly IUserServices _userServices;
            public ResetPasswordUserCommandHandler(IUserServices userServices)
            {
                _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));

            }
            public async Task<UsersDto> Handle(ResetPasswordUserCommand command, CancellationToken cancellationToken)
            {
                UsersDto resultUpdpasswUser = null;
                try
                {
                    Users user = new Users();
                    user.Username = command.Username;
                    user.Email = command.Email;
                    Users aUser = await _userServices.Get_User_byUserNameEmail(user.Username, user.Email);
                    if (aUser == null)//Nếu tài khoản không tồn tại
                    {
                        return new UsersDto();
                    }
                    aUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.NewPassword);
                    resultUpdpasswUser = await _userServices.ChangePassw(aUser);
                    resultUpdpasswUser.Password = command.NewPassword;
                    return resultUpdpasswUser;
                }
                catch
                {
                    return resultUpdpasswUser;
                }
            }

        }
    }
}

