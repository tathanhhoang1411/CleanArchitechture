using AutoMapper;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Application.Services;
using CleanArchitecture.Entites.Enums;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace CleanArchitecture.Application.Features.Users.Commands.Update
{

    public class UpdInfoUserCommand : IRequest<UserWithDetailDto>
    {
            // C# sẽ tự động map dữ liệu từ JSON vào các property này
            [Required]
            public long UserID { get; init; }

            // Nếu Postman không gửi, các giá trị này sẽ nhận mặc định ngay lập tức
            public string FirstName { get; init; } = string.Empty;
            public string LastName { get; init; } = string.Empty;
            public string Address { get; init; } = "N/A";
            public string Bio { get; init; } = "No bio provided.";

            public DateTime BirthDate { get; init; } = new DateTime(1900, 1, 1);

            public MaterialType Material { get; init; } = MaterialType.Single;
            public GenderType Gender { get; init; } = GenderType.Other;

            public int CountryCode { get; init; } = 84;
            public string Phone { get; init; } = string.Empty;
        }

        public class UpdInfoUserCommandHandler : IRequestHandler<UpdInfoUserCommand, UserWithDetailDto>
        {
            private readonly IUserDetailsServices _userDetailsServices;
            private readonly IUserServices _userServices;
            public UpdInfoUserCommandHandler(
                IUserDetailsServices userDetailsServices
                , IUserServices userServices)
            {
                _userDetailsServices = userDetailsServices ?? throw new ArgumentNullException(nameof(userDetailsServices));
                _userServices = userServices ?? throw new ArgumentNullException(nameof(userServices));

            }
            public async Task<UserWithDetailDto> Handle(UpdInfoUserCommand command, CancellationToken cancellationToken)
            {
                UserWithDetailDto userWithDetailDto = null;
                try
                {
                    Entites.Entites.User user = new Entites.Entites.User();
                    user.UserId = command.UserID;
                    Entites.Entites.User isExistUser = await _userServices.CheckExistUser(user);
                    if (isExistUser == null || isExistUser.Status == false)//Nếu tài khoản cần xóa không tồn tại hoặc đang bị vô hiệu hóa
                    {
                        return new UserWithDetailDto();
                    }
                    Entites.Entites.UserDetail userDetail = new Entites.Entites.UserDetail();
                    userDetail.Address = command.Address;
                    userDetail.BirthDate=command.BirthDate;
                    userDetail.FirstName = command.FirstName;
                    userDetail.LastName = command.LastName;
                    userDetail.Material = (int)command.Material;
                    userDetail.Gender = (int)command.Gender;
                    userDetail.Bio=command.Bio;
                    userDetail.CountryCode = command.CountryCode;
                    userDetail.Phone= command.Phone;
                    userDetail.UserId = command.UserID; 
                    userWithDetailDto = await _userDetailsServices.UpdateinfoUser(userDetail);
                    
                    return userWithDetailDto;
                }
                catch
                {
                    return new UserWithDetailDto();
                }
            }

        }
    }

