using AutoMapper;
using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Query
{
    public class LoginQuery : IRequest<string>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public class LoginQueryHandler : IRequestHandler<LoginQuery, string>
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
            public async Task<string> Handle(LoginQuery query, CancellationToken cancellationToken)
            {
                Boolean result = false;
                // Ánh xạ LoginQuery thành UserDto và lưu vào biến
                UserDto userDto = _mapper.Map<UserDto>(query);
                Users user = await _userRepository.Login(userDto);
                if (user == null)//Nếu không có tồn tại tài khoản 
                {
                    return "";
                }
                //Khi này, đăng nhập thành công thì trả về Access token
                string accessToken = _userServices.MakeToken(user);
                if(accessToken=="")
                {
                    return "";
                }
                //Khi này, lưu token trong server
                Boolean isSaveToken = await _userServices.SaveToken(user, accessToken);
                if (isSaveToken == false)
                {
                    return ""; 
                }

                return accessToken;
            }
        }
    }
}
