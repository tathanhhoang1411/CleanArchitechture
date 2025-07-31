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

namespace CleanArchitecture.Application.Commands.Create
{

    public class UserCommand : IRequest<UserDto>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public class CreateUserCommandHandler : IRequestHandler<UserCommand, UserDto>
        {
            private readonly IUserServices _userServices;
            public CreateUserCommandHandler(IUserServices userServices, IUserRepository userRepository)
            {
                _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));

            }
            public async Task<UserDto> Handle(UserCommand command, CancellationToken cancellationToken)
            {
                try
                {
                    DateTime dateTime = DateTime.UtcNow;
                    long timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                    Users user = new Users();
                    user.Username = command.Username;
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);
                    user.Email = command.Email;
                    user.UserId = timestamp;
                    user.CreatedAt = dateTime;
                    user.Role = "User";
                    bool isExistUser = await _userServices.CheckExistUser(user);
                    if (isExistUser)//Nếu đăng kí tài khoản đã tồn tại
                    {
                        return null;
                    }
                    UserDto resultCreateUser = await _userServices.CreateUser(user);
                    string accessToken = _userServices.MakeToken(user);
                    if (accessToken == null || accessToken == "")
                    {
                        return null;
                    }
                    user.Token = accessToken;
                    bool resultSaveToken = await _userServices.SaveToken(user, accessToken);
                    if (resultSaveToken == false)
                    {
                        return null;
                    }
                    return resultCreateUser;
                }
                catch
                {
                    return null;
                }
            }

        }
    }
}

