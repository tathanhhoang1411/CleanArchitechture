using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CleanArchitecture.Entites.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationContext _userContext;
        private readonly int _maxTake = 5000;

        public UserRepository(ApplicationContext userContext)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }
        //Trả về 1 user
        public async Task<Entites.Entites.User> Login(Entites.Entites.User user, CancellationToken cancellationToken = default)
        {
            try
            {
                var auser = await _userContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == user.Username && u.Email == user.Email && u.Status == true, cancellationToken);

                if (auser == null)
                    return null;

                var checkPass = BCrypt.Net.BCrypt.Verify(user.PasswordHash, auser.PasswordHash);
                if (!checkPass)
                    return null;

                return auser;
            }
            catch
            {
                return null;
            }
        }
        public async Task<Boolean> SaveToken(Entites.Entites.User user, string accessToken, CancellationToken cancellationToken = default)
        {
            try
            {
                var userDB = await _userContext.Users.FirstOrDefaultAsync(u => u.Username == user.Username, cancellationToken);
                if (userDB == null)
                    return false;

                userDB.Token = accessToken;
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<Entites.Entites.User> CreateUser(Entites.Entites.User user, CancellationToken cancellationToken = default)
        {
            if (user == null) return null;

            try
            {
                var newUser = new Entites.Entites.User
                {
                    Username = user.Username,
                    Email = user.Email,
                    PasswordHash = user.PasswordHash,
                    Role = user.Role,
                    Avatar = user.Avatar,
                    Status = true,
                    CreatedAt = user.CreatedAt == default ? DateTime.UtcNow : user.CreatedAt
                };

                await _userContext.Users.AddAsync(newUser, cancellationToken);
                return newUser;
            }
            catch
            {
                return null;
            }
        }
        public async Task<Entites.Entites.User> CheckExistUser(Entites.Entites.User user, CancellationToken cancellationToken = default)
        {
            try
            {
                var userDB = await _userContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Email == user.Email, cancellationToken);

                return userDB;
            }
            catch
            {
                return null;
            }
        }
        public async Task<Entites.Entites.User> Get_User_byUserNameEmailAndPassw(string userName,string email, string oldPassWord, CancellationToken cancellationToken = default)
        {
            try
            {
                var userDB = await _userContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == userName && u.Email == email && u.Status == true, cancellationToken);

                if (userDB == null) return null;

                var flag = BCrypt.Net.BCrypt.Verify(oldPassWord, userDB.PasswordHash);
                if (!flag) return null;

                return userDB;
            }
            catch
            {
                return null;
            }
        }
        public async Task<Entites.Entites.User> Get_User_byUserNameEmail(string userName,string email, CancellationToken cancellationToken = default)
        {
            try
            {
                var userDB = await _userContext.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == userName && u.Email == email && u.Status == true, cancellationToken);

                return userDB;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<Entites.Entites.User>> GetListUsers(int skip, int take,string data, CancellationToken cancellationToken = default)
        {
            if (skip < 0) skip = 0;
            if (take <= 0) take = 10;
            take = Math.Min(take, _maxTake);

            try
            {
                List<Entites.Entites.User> query = await _userContext.Users
                    .Where(p=>p.Username.StartsWith(data))
                    .OrderBy(p => p.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);


                return query ?? new List<Entites.Entites.User>();
            }
            catch
            {
                return new List<Entites.Entites.User>();
            }
        }
        public async Task<Entites.Entites.User> ChangePassw(Entites.Entites.User user, CancellationToken cancellationToken = default)
        {
            try
            {
                var aUsers = await _userContext.Users
                    .Where(u => u.Username == user.Username && u.Email == user.Email && u.Status == true)
                    .FirstOrDefaultAsync(cancellationToken);

                if (aUsers == null) return null;

                aUsers.PasswordHash = user.PasswordHash;
                return aUsers;
            }
            catch
            {
                return null;
            }
        }
        //Thật chất là thay đổi trạng thái user
        public async Task<Boolean> DeleteUser(Entites.Entites.User user, CancellationToken cancellationToken = default)
        {
            try
            {
                var users = await _userContext.Users
                    .Where(u => u.Username == user.Username && u.Email == user.Email)
                    .FirstOrDefaultAsync(cancellationToken);

                if (users == null) return false;

                users.Status = false;
                return true; // Trả về true khi xóa thành công
            }
            catch
            {
                return false;
            }
        }
        //Thật chất là thay đổi trạng thái user
        public async Task<Boolean> ActiveUser(Entites.Entites.User user, CancellationToken cancellationToken = default)
        {
            try
            {
                var users = await _userContext.Users
                    .Where(u => u.Username == user.Username && u.Email == user.Email)
                    .FirstOrDefaultAsync(cancellationToken);

                if (users == null) return false;

                users.Status = true;
                return true; // Trả về true khi active thành công
            }
            catch
            {
                return false;
            }
        }
        public async Task<Boolean> isUpdUserAvatar(Entites.Entites.User user, CancellationToken cancellationToken = default)
        {
            try
            {
                var users = await _userContext.Users
                    .Where(u =>u.Email == user.Email)
                    .FirstOrDefaultAsync(cancellationToken);

                if (users == null) return false;

                users.Avatar = user.Avatar;
                return true; // Trả về true khi active thành công
            }
            catch
            {
                return false;
            }
        }

        public async Task<Entites.Entites.User> GetUserWithDetailByIdentifier(string identifier, CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(identifier)) return null;

                // Try parse as long id
                if (long.TryParse(identifier, out var id))
                {
                    return await _userContext.Users
                        .Include(u => u.UserDetail)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(u => u.UserId == id, cancellationToken);
                }

                // Search by username (exact) or email (exact)
                return await _userContext.Users
                    .Include(u => u.UserDetail)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username == identifier || u.Email == identifier, cancellationToken);
            }
            catch
            {
                return null;
            }
        }
    }
}

