using AutoMapper;
using CleanArchitecture.Application.IRepository;
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

namespace CleanArchitecture.Application.Commands
{

    public class UserCommand : IRequest<Users>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public class CreateUserCommandHandler : IRequestHandler<UserCommand, Users>
        {
            private readonly IUserServices _userServices;
            private readonly IUserRepository _userRepository;
            private readonly IMapper _mapper;
            public CreateUserCommandHandler( IUserServices userServices, IUserRepository userRepository,IMapper mapper)
            {
                _userServices = userServices;
                _userRepository = userRepository;
                _mapper = mapper;

            }
            public async Task<Users> Handle(UserCommand command, CancellationToken cancellationToken)
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
                Boolean isExistUser= await _userRepository.CheckExistUser(user);
                if (isExistUser)//Nếu đăng kí tài khoản đã tồn tại
                {
                    return null;
                }
                Users resultCreateUser = await _userRepository.CreateUser(user);
                string accessToken = _userServices.MakeToken(user);
                if(accessToken==null || accessToken == "")
                {
                    return null;
                }
                user.Token = accessToken;
                Boolean resultSaveToken = await _userServices.SaveToken(user,accessToken);
                if (resultSaveToken == false)
                {
                    return null;
                }
                return resultCreateUser;
            }

        }
    }
}
    
