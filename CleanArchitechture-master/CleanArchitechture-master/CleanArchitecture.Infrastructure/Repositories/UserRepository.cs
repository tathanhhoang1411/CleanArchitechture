using AutoMapper;
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
                 user = await _userContext.Users
                    .FirstOrDefaultAsync(u => u.Username == userDto.Username && u.Email == userDto.Email);
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
                // Lấy thông tin người dùng từ cơ sở dữ liệu
                userDB = await _userContext.Users
                   .FirstOrDefaultAsync(u => u.Username == user.Username && u.Email == user.Email );
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
    }
}
