using AutoMapper;
using CleanArchitecture.Application.Interfaces;
using CleanArchitecture.Entites.Entites;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Interfaces;

namespace CleanArchitecture.Application.Services
{
    public class UserDetailsServices : IUserDetailsServices
    {
        private  IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _cache;
        private readonly IRabbitMQService _rabbitMQ;
        public UserDetailsServices(IConfiguration configuration, IUserRepository userRepository, IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheService cache, IRabbitMQService rabbitMQ)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _rabbitMQ = rabbitMQ ?? throw new ArgumentNullException(nameof(rabbitMQ));
        }

        public async Task<UserWithDetailDto> UpdateinfoUser(UserDetail userDetails)
        {
            try
            {
                UserDetail userDeta=await _unitOfWork.UserDetails.UpdateUserDetails(userDetails);
                if (userDeta.UserId == 0) { return new UserWithDetailDto(); }
                await _unitOfWork.CompleteAsync();
                // Gửi event sau khi DB đã cập nhật
                _rabbitMQ.Publish($"UserDetailsUpdate:{userDetails.UserId}");
                return _mapper.Map<UserWithDetailDto>(userDetails);
            }
            catch
            {
                return new UserWithDetailDto();
            }
        }
    }
}
