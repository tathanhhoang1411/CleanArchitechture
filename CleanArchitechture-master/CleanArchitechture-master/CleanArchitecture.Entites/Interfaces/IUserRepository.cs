
using CleanArchitecture.Entites.Entites;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IUserRepository
    {
        Task<User> Login(User user);
        Task<Boolean> SaveToken(User user,string accessToken);
        Task<User> CreateUser(User user);
        Task<Boolean> DeleteUser(User user);
        Task<Boolean> ActiveUser(User user);
        Task<User> CheckExistUser(User user);
        Task<User> ChangePassw(User user);
        Task<User> Get_User_byUserNameEmailAndPassw(string userName,string email, string passWord);
        Task<User> Get_User_byUserNameEmail(string userName,string email);
        Task<List<User>> GetListUsers(int skip, int take, string data);
    }
}
