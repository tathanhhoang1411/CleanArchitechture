using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IUserServices
    {
        string MakeToken(User user);
        Task<Boolean> SaveToken(User user, string accessToken);
        Task<User> CheckExistUser(User user);
        Task<UsersDto> CreateUser(User user);
        Task<UsersDto> DelUser(User user);
        Task<UsersDto> ActiveUser(User user);
        Task<long> GetUserIDInTokenFromRequest(string tokenJWT);
        ClaimsPrincipal ValidateToken( string accessToken);
        Task<List<UsersDto>> GetList_Users(int skip, int take, string data);
        Task<User> Get_User_byUserNameEmailAndPassw(string userName,string email, string passWord);
        Task<User> Get_User_byUserNameEmail(string userName,string email);
        Task<UsersDto> ChangePassw(User user);

    }
}
