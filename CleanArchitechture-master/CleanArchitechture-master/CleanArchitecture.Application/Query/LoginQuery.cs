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

namespace CleanArchitecture.Application.Query
{
    public class LoginQuery : IRequest<Boolean>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public class LoginQueryHandler : IRequestHandler<LoginQuery, Boolean>
        {

            private readonly IUserRepository _userRepository;
            private readonly IMapper _mapper;
            private readonly IUserServices _userServices;
            public LoginQueryHandler(IUserRepository userRepository,IMapper mapper,IUserServices userServices)
            {
                _userRepository = userRepository;
                _mapper = mapper;
                _userServices = userServices;
            }
            public async Task<Boolean> Handle(LoginQuery query, CancellationToken cancellationToken)
            {
                Boolean result = false;
                // Ánh xạ LoginQuery thành UserDto và lưu vào biến
                UserDto userDto = _mapper.Map<UserDto>(query);
                Users user = await _userRepository.Login(userDto);
                if (user == null)//Nếu không có tồn tại tài khoản 
                {
                    return false;
                }
                result = await _userServices.CheckPassword(userDto.Password, user.PasswordHash);
                if (result==false)//Nếu không đúng password 
                {
                    return false; 
                }
                //Khi này, đăng nhập thành công thì trả về Access token
                string token = _userServices.MakeToken(user);
                 return true;
            }
        }
    }
}
