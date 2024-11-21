using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Domain.Interfaces.Common
{
    public interface IUnitOfWork
    {
        //IRepository<T> GetRepository<T>() where T : class;
        //Task<int> SaveChangesAsync(CancellationToken cancellationToken = default); // hủy bỏ bất đồng bộ nếu cần
        //Task BeginTransactionAsync();
        //Task CommitAsync();
        //Task RollbackAsync();
    }
}
