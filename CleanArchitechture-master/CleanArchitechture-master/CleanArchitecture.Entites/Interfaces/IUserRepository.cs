using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IUserRepository
    {
        Task<User> Login(User user, CancellationToken cancellationToken = default);
        Task<Boolean> SaveToken(User user,string accessToken, CancellationToken cancellationToken = default);
        Task<User> CreateUser(User user, CancellationToken cancellationToken = default);
        Task<Boolean> DeleteUser(User user, CancellationToken cancellationToken = default);
        Task<Boolean> ActiveUser(User user, CancellationToken cancellationToken = default);
        Task<User> CheckExistUser(User user, CancellationToken cancellationToken = default);
        Task<User> ChangePassw(User user, CancellationToken cancellationToken = default);
        Task<User> Get_User_byUserNameEmailAndPassw(string userName,string email, string passWord, CancellationToken cancellationToken = default);
        Task<User> Get_User_byUserNameEmail(string userName,string email, CancellationToken cancellationToken = default);
        Task<List<User>> GetListUsers(int skip, int take, string data, CancellationToken cancellationToken = default);
    }
}
