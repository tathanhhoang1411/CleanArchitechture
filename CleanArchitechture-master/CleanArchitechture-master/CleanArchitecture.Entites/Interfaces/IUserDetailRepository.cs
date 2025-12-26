using CleanArchitecture.Entites.Entites;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IUserDetailRepository
    {
        Task<UserDetail> UpdateUserDetails(UserDetail userDetails, CancellationToken cancellationToken = default);
    }
}
