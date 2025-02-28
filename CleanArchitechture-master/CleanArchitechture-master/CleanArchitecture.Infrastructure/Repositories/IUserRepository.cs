using CleanArchitecture.Entites.Dtos;
using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public interface IUserRepository
    {
        Task<Users> Login(UserDto user);
        Task<Boolean> SaveToken(Users user,string accessToken);
        Task<Users> CreateUser(Users user);
        Task<Boolean> CheckExistUser(Users user);
        Task<List<UserDto>> GetListUsers(int skip, int take, string data);
    }
}
