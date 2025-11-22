using AutoMapper;
using CleanArchitecture.Application.Interfaces;
using MediatR;
using CleanArchitecture.Entites.Interfaces;

namespace CleanArchitecture.Application.Features.Users.Query
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
                _mapper = mapper;
                _userServices = userServices;
                _userRepository = userRepository;
            }
            public async Task<string> Handle(LoginQuery query, CancellationToken cancellationToken)
            {
                Boolean result = false;
                // Ánh xạ LoginQuery thành UserDto và lưu vào biến
                Entites.Entites.User aUser = new Entites.Entites.User();
                aUser.Username = query.Username;
                aUser.PasswordHash = query.Password;
                aUser.Email = query.Email;
                Entites.Entites.User user = await _userRepository.Login(aUser);
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
