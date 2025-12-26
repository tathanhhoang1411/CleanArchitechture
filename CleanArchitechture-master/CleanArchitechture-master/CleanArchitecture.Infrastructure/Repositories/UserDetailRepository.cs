using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class UserDetailRepository : IUserDetailRepository
    {
        private readonly ApplicationContext _userContext;
        private readonly int _maxTake = 5000;

        public UserDetailRepository(ApplicationContext userContext)
        {
            _userContext = userContext ?? throw new ArgumentNullException(nameof(userContext));
        }
        
        public async Task<UserDetail> UpdateUserDetails(UserDetail userDetails, CancellationToken cancellationToken = default)
        {
            try
            {
                var userDeta = await _userContext.UserDetails
                    .Where(u => u.UserId == userDetails.UserId )
                    .FirstOrDefaultAsync(cancellationToken);

                if (userDeta == null) return new UserDetail();

                userDeta.BirthDate = userDetails.BirthDate;
                userDeta.FirstName = userDetails.FirstName.Trim();
                userDeta.LastName = userDetails.LastName.Trim();
                userDeta.Address = userDetails.Address.Trim();
                userDeta.Bio = userDetails.Bio;
                userDeta.CountryCode = userDetails.CountryCode;
                userDeta.Gender = userDetails.Gender;
                userDeta.Material = userDetails.Material;
                userDeta.UserId = userDetails.UserId;
                userDeta.Phone = userDetails.Phone.Trim();
                return userDetails;
            }
            catch
            {
                return new UserDetail();
            }
        }
       
    }
}

