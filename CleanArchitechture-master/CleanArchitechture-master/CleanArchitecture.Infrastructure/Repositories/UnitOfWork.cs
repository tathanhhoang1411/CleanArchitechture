using CleanArchitecture.Entites.Entites;
using CleanArchitecture.Entites.Interfaces;
using CleanArchitecture.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Threading;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork,IDisposable
    {
        private readonly ApplicationContext _context;
        private bool _disposed = false;

        public IProductRepository Products { get; private set; }
        public IReviewRepository Reviews { get; private set; }
        public IUserRepository Users{ get; private set; }
        public IUserDetailRepository UserDetails{ get; private set; }
        public ICommentRepository Comments { get; private set; }
        public IFriendRepository Friends { get; private set; }
        public UnitOfWork(ApplicationContext context, IConfiguration configuration)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Products = new ProductRepository(_context, configuration);
            Reviews = new ReviewRepository(_context, configuration);
            Users = new UserRepository(_context);
            Comments = new CommentRepository(_context, configuration);
            Friends = new FriendRepository(_context, configuration);
            UserDetails = new UserDetailRepository(_context);
        }


        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                // Xử lý lỗi nếu cần thiết
                throw new Exception("An error occurred while saving changes.", ex);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                _disposed = true;
            }
        }
    }
}