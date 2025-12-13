
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Dtos;
using MediatR;

namespace CleanArchitecture.Application.Features.Users.Commands.Create
{

    public class UserCommand : IRequest<UsersDto>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? Avatar { get; set; }
        public class CreateUserCommandHandler : IRequestHandler<UserCommand, UsersDto>
        {
            private readonly IUserServices _userServices;
            public CreateUserCommandHandler(IUserServices userServices)
            {
                _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));

            }
            public async Task<UsersDto> Handle(UserCommand command, CancellationToken cancellationToken)
            {
                try
                {
                    DateTime dateTime = DateTime.UtcNow;
                    long timestamp = new DateTimeOffset(dateTime).ToUnixTimeSeconds();
                    Entites.Entites.User user = new Entites.Entites.User();
                    user.Username = command.Username;
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);
                    user.Email = command.Email;
                    user.UserId = timestamp;
                    user.CreatedAt = dateTime;
                    user.Role = "User";
                    user.Avatar = command.Avatar;
                    Entites.Entites.User isExistUser = await _userServices.CheckExistUser(user);
                    if (isExistUser!=null)//Nếu đăng kí tài khoản đã tồn tại
                    {
                        return null;
                    }
                    UsersDto resultCreateUser = await _userServices.CreateUser(user);
                    string accessToken = _userServices.MakeToken(user);
                    if (accessToken == null || accessToken == "")
                    {
                        return new UsersDto();
                    }
                    user.Token = accessToken;
                    bool resultSaveToken = await _userServices.SaveToken(user, accessToken);
                    if (resultSaveToken == false)
                    {
                        return new UsersDto();
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

