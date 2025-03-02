using AutoMapper;
using BCrypt.Net;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace CleanArchitecture.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationContext _userContext;
        private readonly IMapper _mapper;
        public UserRepository(ApplicationContext userContext, IMapper mapper)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        //Trả về 1 user
        public async Task<Users> Login(UserDto userDto)
        {
            var user = new Users();
            try
            {
                // Lấy thông tin người dùng từ cơ sở dữ liệu
                user = await _userContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(
                    u => u.Username 
                    == 
                    userDto.Username 
                    &&
                    u.Email 
                    ==
                    userDto.Email);
                if (user==null)
                {
                    return null;
                }
                Boolean checkPass= BCrypt.Net.BCrypt.Verify(userDto.Password, user.PasswordHash);
                if (checkPass==false)
                {
                    return null;
                }
                return user;
            }
            catch
            {
                return user;
            }
        }
        public async Task<Boolean> SaveToken(Users user, string accessToken)
        {
            Users userDB = new Users();
            try
            {
                userDB = await _userContext.Users
                    .FirstOrDefaultAsync(
                    u => u.Username 
                    ==
                    user.Username 
                    && 
                    u.PasswordHash
                    ==
                    user.PasswordHash);
                if (userDB == null)
                {
                    return false;
                }
                // Cập nhật các thuộc tính của đối tượng
                userDB.Token = accessToken;

                // Lưu thay đổi vào cơ sở dữ liệu
                await _userContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<Users> CreateUser(Users user)
        {
            var NewUser = new Users();
            try
            {
                NewUser.Username = user.Username;
                NewUser.Email = user.Email;
                NewUser.CreatedAt = user.CreatedAt;
                NewUser.PasswordHash = user.PasswordHash;
                NewUser.UserId = user.UserId;
                NewUser.Role = user.Role;
                _userContext.Users.Add( NewUser );
               await  _userContext.SaveChangesAsync();

                    return user;
            }
            catch
            {
                return user;
            }
        }
        public async Task<Boolean> CheckExistUser(Users user)
        {
            Users userDB = new Users();
            try
            {
                userDB = await _userContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                    u => u.Username 
                    ==
                    user.Username 
                    &&
                    u.Email
                    == 
                    user.Email);
                if (userDB == null)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<UserDto>> GetListUsers(int skip, int take,string data)
        {
                var users = await _userContext.Users
                .Where(u => u.Username.Contains(data))
                .Take(take)
                .Skip(skip)
                .OrderBy(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
                return _mapper.Map<List<UserDto>>(users);
        }
    }
}

