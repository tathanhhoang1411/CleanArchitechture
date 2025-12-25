using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Features.Users.Commands.Update
{
    public class UpdateUserAvatarCommand : IRequest<UsersDto>
    {
        public long userID { get; set; }
        public string avatarPath { get; set; }
        public string email { get; set; }
        public class UpdateUserAvatarCommandHandler : IRequestHandler<UpdateUserAvatarCommand, UsersDto>
        {
            private readonly IUserServices _userServices;
            public UpdateUserAvatarCommandHandler(IUserServices userServices)
            {
                _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));

            }
            public async Task<UsersDto> Handle(UpdateUserAvatarCommand command, CancellationToken cancellationToken)
            {
                UsersDto resultDelUser = new UsersDto();
                try
                {
                    Entites.Entites.User user = new Entites.Entites.User();
                    user.UserId = command.userID;
                    user.Avatar = command.avatarPath;
                    user.Email = command.email;
                    Entites.Entites.User isExistUser = await _userServices.CheckExistUser(user);
                    if (isExistUser == null)//Nếu tài khoản cần xóa không tồn tại
                    {
                        return new UsersDto();
                    }
                    resultDelUser = await _userServices.UpdateUserAvatar(user);
                    resultDelUser.Avatar = command.avatarPath;
                    return resultDelUser;
                }
                catch
                {
                    return new UsersDto();
                }
            }

        }
    }
}
