using AutoMapper;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Dtos;
using MediatR;

namespace CleanArchitecture.Application.Features.Users.Commands.Update
{

    public class UpdPasswordUserCommand : IRequest<UsersDto>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string OldPass { get; set; }
        public class UpdPasswordUserCommandHandler : IRequestHandler<UpdPasswordUserCommand, UsersDto>
        {
            private readonly IUserServices _userServices;
            public UpdPasswordUserCommandHandler(IUserServices userServices)
            {
                _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));

            }
            public async Task<UsersDto> Handle(UpdPasswordUserCommand command, CancellationToken cancellationToken)
            {
                UsersDto resultUpdpasswUser = null;
                try
                {
                    Entites.Entites.User user = new Entites.Entites.User();
                    user.Username = command.Username;
                    user.Email = command.Email;
                    Entites.Entites.User aUser = await _userServices.Get_User_byUserNameEmailAndPassw(user.Username, user.Email, command.OldPass);
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

