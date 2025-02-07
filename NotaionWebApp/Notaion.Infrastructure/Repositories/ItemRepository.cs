using Notaion.Domain.Entities;
using Notaion.Domain.Interfaces;
using Notaion.Infrastructure.Context;

namespace Notaion.Infrastructure.Repositories;

public class ItemRepository : GenericRepository<Item>, IItemRepository
{
    private readonly ApplicationDbContext _context;
    public ItemRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }
}