using Microsoft.EntityFrameworkCore.Storage;
using Notaion.Domain.Interfaces;
using Notaion.Infrastructure.Context;

namespace Notaion.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _currentTransaction;
        private readonly Dictionary<Type, object> _repositories;
        public IChatRepository ChatRepository => new ChatRepository(_context);
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            _repositories = new Dictionary<Type, object>();
        }

        public IGenericRepository<T> GetGenericRepository<T>() where T : class
        {
            if (_repositories.ContainsKey(typeof(T)))
            {
                return _repositories[typeof(T)] as IGenericRepository<T>;
            }
            var repository = new GenericRepository<T>(_context);
            _repositories.Add(typeof(T), repository);
            return repository;
        }

        public Task<int> SaveChangeAsync()
        {
            return _context.SaveChangesAsync();
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task BeginTransactionAsync()
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _currentTransaction.CommitAsync();
            }
            catch
            {
                await _currentTransaction.RollbackAsync();
                return;
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null!;
            }

        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private bool dispose = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!this.dispose)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
                this.dispose = true;
            }
        }
    }
}
