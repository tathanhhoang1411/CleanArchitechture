using CleanArchitecture.Entites.Entites;
using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }

        Task<int> CompleteAsync();
    }
}
