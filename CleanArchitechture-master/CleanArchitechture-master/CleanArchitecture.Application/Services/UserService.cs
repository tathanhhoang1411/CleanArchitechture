using AutoMapper;
using AutoMapper.Configuration;
using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using CleanArchitecture.Entites.Dtos;
using Org.BouncyCastle.Asn1.Ocsp;
namespace CleanArchitecture.Application.Services
{
    public class UserService:IUserServices
    {
        //public string HashPassword(string password)
        //{
        //    return BCrypt.Net.BCrypt.HashPassword(password);
        //}

        //public void RegisterUser(string password)
        //{
        //    string hashedPassword = HashPassword(password);
        //    // Lưu hashedPassword vào cơ sở dữ liệua
        //}
        private  IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        public UserService(IConfiguration configuration, IUserRepository userRepository)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _userRepository = userRepository;
        }

        public string MakeToken(Users user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Thêm claims cho vai trò
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
        new Claim(JwtRegisteredClaimNames.NameId, user.UserId.ToString()), // Sử dụng NameId cho ID
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.UniqueName,user.Email),
        new Claim(ClaimTypes.Role, user.Role.Trim()) // Gán vai trò từ đối tượng user
    };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: credentials);

            var encodeToken = new JwtSecurityTokenHandler().WriteToken(token);
            return encodeToken;
        }
        public Task<Boolean> SaveToken(Users user, string accessToken)
        {

            return _userRepository.SaveToken(user, accessToken);
        }     
        public async Task<long> GetUserIDInTokenFromRequest(string tokenJWT)
        {
            long result=0;
            try
            {
                // Giải mã token
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(tokenJWT);

                // Lấy ID từ payload
                var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "nameid")?.Value; // Thay "id" bằng tên trường bạn sử dụng
                var email = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name")?.Value; // Thay "id" bằng tên trường bạn sử dụng
                var userName = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value; // Thay "id" bằng tên trường bạn sử dụng
                Users users = new Users();
                users.Username = userName;
                users.UserId = long.Parse(userId);
                users.Email = email;
                Task<bool> checkUser=_userRepository.CheckExistUser(users);
                // Đợi để lấy giá trị bool từ Task
                bool resultFromTask = await checkUser;
                if (!resultFromTask)
                {
                    return result;
                }
                result = long.Parse(userId);
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }
        public ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                // Giải mã và xác thực token
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true, // Có thể cấu hình theo nhu cầu
                    ValidateAudience = true, // Có thể cấu hình theo nhu cầu
                    ClockSkew = TimeSpan.Zero, // Không cho phép thời gian trễ
                    ValidateLifetime = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"]
                }, out SecurityToken validatedToken);

                // Token hợp lệ và chưa hết hạn
                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                // Token đã hết hạn
                return null;
            }
            catch (Exception)
            {
                // Token không hợp lệ
                return null;
            }
        }
        public Task<List<UserDto>> GetList_Users(int skip, int take,  string data)
        {
            return _userRepository.GetListUsers(skip, take,data);
        }
    }
}
