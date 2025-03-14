using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Application.IRepository
{
    public interface IUserServices
    {
        string MakeToken(Users user);
        Task<Boolean> SaveToken(Users user, string accessToken);
        Task<long> GetUserIDInTokenFromRequest(string tokenJWT);
        ClaimsPrincipal ValidateToken( string accessToken);
        Task<List<UserDto>> GetList_Users(int skip, int take, string data);

    }
}
