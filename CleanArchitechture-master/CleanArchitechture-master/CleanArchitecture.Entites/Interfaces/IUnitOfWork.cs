using CleanArchitecture.Entites.Entites;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IReviewRepository Reviews { get; }
        IUserRepository Users { get; }
        IUserDetailRepository UserDetails { get; }
        ICommentRepository Comments { get; }
        IChatRepository Chat { get; } // Add Chat Repository
        IFriendRepository Friends { get; }

        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
    }
}
