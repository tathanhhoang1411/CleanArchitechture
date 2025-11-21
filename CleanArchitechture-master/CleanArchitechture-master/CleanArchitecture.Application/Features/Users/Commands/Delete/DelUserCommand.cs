using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Dtos;
using MediatR;
using System;

namespace CleanArchitecture.Application.Features.Users.Commands.Delete
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
                    Entites.Entites.User user = new Entites.Entites.User();
                    user.Username = command.Username;
                    user.Email = command.Email;
                    Entites.Entites.User isExistUser = await _userServices.CheckExistUser(user);
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

