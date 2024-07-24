using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notaion.Models;

namespace Notaion.Context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }
    }
}
