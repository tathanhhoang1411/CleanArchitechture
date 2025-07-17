using CleanArchitecture.Infrastructure.DBContext;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationContext _context;
        private IDbContextTransaction _transaction;

        public UnitOfWork(ApplicationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task BeginTransactionAsync()
        {
            try
            {
                _transaction = await _context.Database.BeginTransactionAsync();
            }
            catch (Exception ex)
            {
                // Log lỗi ở đây nếu cần
                throw new InvalidOperationException("Could not begin transaction.", ex);
            }
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    await _transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    // Log lỗi ở đây nếu cần
                    throw new InvalidOperationException("Could not commit transaction.", ex);
                }
            }
            else
            {
                throw new InvalidOperationException("Transaction has not been started.");
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                try
                {
                    await _transaction.RollbackAsync();
                }
                catch (Exception ex)
                {
                    // Log lỗi ở đây nếu cần
                    throw new InvalidOperationException("Could not rollback transaction.", ex);
                }
                finally
                {
                    _transaction.Dispose(); // Giải phóng tài nguyên
                    _transaction = null; // Đặt lại biến
                }
            }
        }
    }
}