using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notaion.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        public IChatRepository ChatRepository { get; }
        public IItemRepository ItemRepository { get; }

        IGenericRepository<T> GetGenericRepository <T>() where T : class;
        Task<int> SaveChangeAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
