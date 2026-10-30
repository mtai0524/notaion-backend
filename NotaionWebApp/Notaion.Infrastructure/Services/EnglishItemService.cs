using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Notaion.Application.Interfaces.Services;
using Notaion.Application.Services;
using Notaion.Domain.Entities;
using Notaion.Infrastructure.Context;

namespace Notaion.Infrastructure.Services
{
    public class EnglishItemService : IEnglishItemService
    {
        private readonly ApplicationDbContext _context;

        public EnglishItemService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Dictionary<string, List<JsonObject>>> GetAllAsync(string userId)
        {
            var rows = await _context.EnglishItems
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return EnglishItemRules.Kinds.ToDictionary(
                kind => kind,
                kind => rows.Where(e => e.Kind == kind).Select(e => EnglishItemRules.ToItem(e.Id, e.CreatedAt, e.Data)).ToList());
        }

        public async Task<JsonObject> SaveAsync(string userId, string kind, JsonObject item)
        {
            var row = await UpsertAsync(userId, kind, item);
            await _context.SaveChangesAsync();
            return EnglishItemRules.ToItem(row.Id, row.CreatedAt, row.Data);
        }

        public async Task<int> SaveManyAsync(string userId, Dictionary<string, List<JsonObject>> itemsByKind)
        {
            // Load the existing rows in one query; UpsertAsync then finds them in Local.
            var ids = itemsByKind.Values.SelectMany(items => items).Select(EnglishItemRules.ReadId).ToList();
            await _context.EnglishItems.Where(e => e.UserId == userId && ids.Contains(e.Id)).LoadAsync();

            var count = 0;
            foreach (var (kind, items) in itemsByKind)
            {
                foreach (var item in items)
                {
                    await UpsertAsync(userId, kind, item);
                    count++;
                }
            }
            await _context.SaveChangesAsync();
            return count;
        }

        public async Task<bool> DeleteAsync(string userId, string kind, string id)
        {
            var row = await _context.EnglishItems.FirstOrDefaultAsync(e => e.UserId == userId && e.Id == id && e.Kind == kind);
            if (row == null) return false;

            _context.EnglishItems.Remove(row);
            await _context.SaveChangesAsync();
            return true;
        }

        // Last write wins; an existing id keeps its creation time.
        private async Task<EnglishItem> UpsertAsync(string userId, string kind, JsonObject item)
        {
            var id = EnglishItemRules.ReadId(item);
            var now = DateTime.UtcNow;
            var row = _context.EnglishItems.Local.FirstOrDefault(e => e.UserId == userId && e.Id == id)
                ?? await _context.EnglishItems.FirstOrDefaultAsync(e => e.UserId == userId && e.Id == id);

            if (row == null)
            {
                row = new EnglishItem
                {
                    UserId = userId,
                    Id = id,
                    CreatedAt = EnglishItemRules.ReadCreatedAt(item, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()),
                };
                _context.EnglishItems.Add(row);
            }

            row.Kind = kind;
            row.Data = EnglishItemRules.ToData(item);
            row.UpdatedAt = now;
            return row;
        }
    }
}
