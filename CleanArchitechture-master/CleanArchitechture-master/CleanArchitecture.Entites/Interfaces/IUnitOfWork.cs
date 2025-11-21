using CleanArchitecture.Entites.Entites;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Entites.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IReviewRepository Reviews { get; }
        IUserRepository Users { get; }
        ICommentRepository Comments { get; }

        Task<int> CompleteAsync();
    }
}
