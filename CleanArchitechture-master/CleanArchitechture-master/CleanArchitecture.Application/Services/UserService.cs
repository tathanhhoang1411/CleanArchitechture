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
    public class UserService:IUserServices
    {
        private  IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRedisCacheService _cache;
        private readonly RabbitMQService _rabbitMQ;
        public UserService(IConfiguration configuration, IUserRepository userRepository, IUnitOfWork unitOfWork, IMapper mapper, IRedisCacheService cache, RabbitMQService rabbitMQ)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _rabbitMQ = rabbitMQ ?? throw new ArgumentNullException(nameof(rabbitMQ));
        }

        public string MakeToken(User user)
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
        public async Task<Boolean> SaveToken(User user, string accessToken)
        {
            Boolean result= false;
            try
            {
                result=await _unitOfWork.Users.SaveToken(user, accessToken);
                if (!result) { return result; }
                await _unitOfWork.CompleteAsync();
                return result;
            }
            catch
            {
                return result;
            }
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
                User users = new User();
                users.Username = userName;
                users.UserId = long.Parse(userId);
                users.Email = email;
                Task<User> checkUser= _unitOfWork.Users.CheckExistUser(users);
                // Đợi để lấy giá trị bool từ Task
                User resultFromTask = await checkUser;
                if (!resultFromTask.Status)
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
        public async Task<List<UsersDto>> GetList_Users(int skip, int take,  string data)
        {
            var cacheKey = $"users:skip:{skip}:take:{take}:q:{data}";
            var cached = await _cache.GetAsync<List<UsersDto>>(cacheKey);
            if (cached != null)
                return cached;
            try
            {
                List<User> listUser =await _unitOfWork.Users.GetListUsers(skip, take, data);
                //
                await _cache.SetAsync(cacheKey, listUser, TimeSpan.FromMinutes(1));
                return _mapper.Map<List<UsersDto>>(listUser);
            }
            catch
            {
                return null;
            }
        }
        public async Task<User> CheckExistUser(User user)
        {
            User aUser = null;
            try
            {
                aUser = await _unitOfWork.Users.CheckExistUser(user);
                if (aUser!= null)
                {
                    return aUser;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }      
        public async Task<User> Get_User_byUserNameEmailAndPassw(string userName,string email, string passWord)
        {
            try
            {
                 User aUser = await _unitOfWork.Users.Get_User_byUserNameEmailAndPassw(userName, email, passWord);
                return aUser;
            }
            catch
            {
                return null;
            }
        }     
        public async Task<User> Get_User_byUserNameEmail(string userName,string email)
        {
            try
            {

                User aUser= await _unitOfWork.Users.Get_User_byUserNameEmail(userName, email);
                return aUser;
            }
            catch
            {
                return null;
            }
        }
        public async Task<UsersDto> CreateUser(User user)
        {

            UsersDto userDto = null;
            try
            {
                 await _unitOfWork.Users.CreateUser(user);
                await _unitOfWork.CompleteAsync();
                // Gửi event sau khi DB đã cập nhật
                _rabbitMQ.Publish($"UsersCreate:{user.UserId}");
                return _mapper.Map<UsersDto>(user);
            }
            catch
            {
                return userDto;
            }
        }         
        public async Task<UsersDto> ChangePassw(User user)
        {

            UsersDto aUser = null;
            try
            {
                await _unitOfWork.Users.ChangePassw(user);
                await _unitOfWork.CompleteAsync();
                // Gửi event sau khi DB đã cập nhật
                _rabbitMQ.Publish($"UsersUpdate:{user.UserId}");
                return _mapper.Map<UsersDto>(user);
            }
            catch
            {
                return aUser;
            }
        }      
        public async Task<UsersDto> DelUser(User user)
        {
            UsersDto userDto = null;
            try
            {
                await _unitOfWork.Users.DeleteUser(user);
                user.Status = false;
                await _unitOfWork.CompleteAsync();
                // Gửi event sau khi DB đã cập nhật
                _rabbitMQ.Publish($"UsersDelete:{user.UserId}");
                return _mapper.Map<UsersDto>(user);
            }
            catch
            {
                return userDto;
            }
            
        }
        public async Task<UsersDto> ActiveUser(User user)
        {
            UsersDto userDto = null;
            try
            {
                await _unitOfWork.Users.ActiveUser(user);
                user.Status = true;
                await _unitOfWork.CompleteAsync();
                // Gửi event sau khi DB đã cập nhật
                _rabbitMQ.Publish($"UsersUpdate:{user.UserId}");
                return _mapper.Map<UsersDto>(user);
            }
            catch
            {
                return userDto;
            }
            
        }

    }
}
