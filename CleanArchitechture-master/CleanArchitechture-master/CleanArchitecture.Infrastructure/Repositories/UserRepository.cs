
using CleanArchitecture.Entites.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
        public async Task<Entites.Entites.User> Login(Entites.Entites.User user)
        {
            Entites.Entites.User? auser = null;
            try
            {
                // Lấy thông tin người dùng từ cơ sở dữ liệu
                auser = await _userContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(
                    u => u.Username 
                    == 
                    user.Username 
                    &&
                    u.Email 
                    ==
                    user.Email
                    &&
                    u.Status==true);
                if (auser==null)
                {
                    return null;
                }
                Boolean checkPass= BCrypt.Net.BCrypt.Verify(user.PasswordHash, auser.PasswordHash);
                if (checkPass==false)
                {
                    return null;
                }
                return auser;
            }
            catch
            {
                return null;
            }
        }
        public async Task<Boolean> SaveToken(Entites.Entites.User user, string accessToken)
        {
            Entites.Entites.User? userDB = null;
            try
            {
                userDB = await _userContext.Users
                    .FirstOrDefaultAsync(
                    u => u.Username 
                    ==
                    user.Username );
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
        public async Task<Entites.Entites.User> CreateUser(Entites.Entites.User user)
        {
            var NewUser = new Entites.Entites.User();
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
        public async Task<Entites.Entites.User> CheckExistUser(Entites.Entites.User user)
        {
            Entites.Entites.User? userDB = null;
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
        public async Task<Entites.Entites.User> Get_User_byUserNameEmailAndPassw(string userName,string email, string oldPassWord)
        {
            Entites.Entites.User? userDB = null;
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
        public async Task<Entites.Entites.User> Get_User_byUserNameEmail(string userName,string email)
        {
            Entites.Entites.User? userDB = null;
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

        public async Task<List<Entites.Entites.User>> GetListUsers(int skip, int take,string data)
        {
            try
            {
                                var users = await _userContext.Users
                .Where(u => u.Username==data)
                .Skip(skip)
                .Take(Math.Min(take, 5000)) // Giới hạn số lượng bản ghi lấy tối đa
                .OrderBy(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
                if (users.Count==0)
                {
                    return new List<Entites.Entites.User>();
                }
                return new List<Entites.Entites.User>();
            }
            catch
            {
                return null;
            }
        }         
        public async Task<Entites.Entites.User> ChangePassw(Entites.Entites.User user)
        {
            Entites.Entites.User? aUsers = null;
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
        public async Task<Boolean> DeleteUser(Entites.Entites.User user)
        {
            Entites.Entites.User? users = null;
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
        public async Task<Boolean> ActiveUser(Entites.Entites.User user)
        {
            Entites.Entites.User? users = null;
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

