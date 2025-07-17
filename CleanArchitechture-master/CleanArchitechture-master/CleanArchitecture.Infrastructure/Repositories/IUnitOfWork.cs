using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Infrastructure.Repositories
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();  // Bắt đầu giao dịch
        Task CommitAsync();            // Xác nhận giao dịch
        Task RollbackAsync();          // Hoàn tác giao dịch
    }
}
