using CleanArchitecture.Application.Dtos;
using CleanArchitecture.Entites.Entites;
using System.Security.Claims;

namespace CleanArchitecture.Application.Interfaces
{
    public interface IUserDetailsServices
    {
        Task<UserWithDetailDto> UpdateinfoUser(UserDetail userDetails);
    }
}
