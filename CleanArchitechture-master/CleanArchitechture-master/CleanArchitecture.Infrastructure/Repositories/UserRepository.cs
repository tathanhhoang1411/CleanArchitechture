using AutoMapper;
using BCrypt.Net;
using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
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
        public UserRepository(ApplicationContext userContext)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
            
        }
        //Trả về 1 user
        public async Task<Users> Login(UsersDto userDto)
        {
            Users? user = null;
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
                    userDto.Email
                    &&
                    u.Status==true);
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
            Users? userDB = null;
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
                    user.PasswordHash );
                if (userDB == null)
                {
                    return false;
                }
                // Cập nhật các thuộc tính của đối tượng
                userDB.Token = accessToken;
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
                NewUser.Status = true;
                _userContext.Users.Add( NewUser );

                    return user;
            }
            catch
            {
                return user;
            }
        }
        public async Task<Users> CheckExistUser(Users user)
        {
            Users? userDB = null;
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
                    return userDB;
                }

                return userDB;
            }
            catch
            {
                return userDB;
            }
        }
        public async Task<Users> Get_User_byUserNameEmailAndPassw(string userName,string email, string oldPassWord)
        {
            Users? userDB = null;
            try
            {

                userDB = await _userContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                    u => u.Username
                    ==
                    userName
                    &&
                    u.Email
                    ==
                    email
                    &&
                    u.Status == true);
                if (userDB == null)
                {
                    return null;
                }
                Boolean flag = BCrypt.Net.BCrypt.Verify(oldPassWord, userDB.PasswordHash);
                if (!flag)
                {
                    return null;
                }
                

                return userDB;
            }
            catch
            {
                return null;
            }
        }     
        public async Task<Users> Get_User_byUserNameEmail(string userName,string email)
        {
            Users? userDB = null;
            try
            {

                userDB = await _userContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                    u => u.Username
                    ==
                    userName
                    &&
                    u.Email
                    ==
                    email
                    &&
                    u.Status == true);
                if (userDB == null)
                {
                    return null;
                }
                return userDB;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Users>> GetListUsers(int skip, int take,string data)
        {
            try
            {
                                var users = await _userContext.Users
                .Where(u => u.Username.Trim().Contains(data))
                .Take(take)
                .Skip(skip)
                .OrderBy(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
                if (users.Count==0)
                {
                    return new List<Users>();
                }
                return new List<Users>(users);
            }
            catch
            {
                return null;
            }
        }         
        public async Task<Users> ChangePassw(Users user)
        {
            Users? aUsers = null;
            try
            {
                 aUsers = await _userContext.Users
.Where(u => u.Username == user.Username && u.Email == user.Email && u.Status ==true)
.FirstOrDefaultAsync();
                if (aUsers == null)
                {
                    return null;
                }
                aUsers.PasswordHash = user.PasswordHash;
                return aUsers;
            }
            catch
            {
                return null;
            }
        }    
        //Thật chất là thay đổi trạng thái user
        public async Task<Boolean> DeleteUser(Users user)
        {
            Users? users = null;
            try
            {
                                 users = await _userContext.Users
                .Where(u => u.Username == user.Username && u.Email == user.Email)
                .FirstOrDefaultAsync();
                if (users == null)
                {
                    return false;
                }
                users.Status = false;

                return true; // Trả về true khi xóa thành công
            }
            catch
            {
                return false;
            }
        } 
        //Thật chất là thay đổi trạng thái user
        public async Task<Boolean> ActiveUser(Users user)
        {
            Users? users = null;
            try
            {
                         users  = await _userContext.Users
            .Where(u => u.Username == user.Username && u.Email == user.Email)
            .FirstOrDefaultAsync();
                if (users == null)
                {
                    return false;
                }
                users.Status = true;

            return true; // Trả về true khi active thành công
        }
            catch
            {
                return false;
            }
        }
    }
}

