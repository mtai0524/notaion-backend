using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Notaion.Domain.Entities;
using Notaion.Domain.Models;


namespace Notaion.Infrastructure.Context
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Page> Page { get; set; }
        public DbSet<Chat> Chat { get; set; }
        public DbSet<ChatPrivate> ChatPrivate { get; set; }
        public DbSet<Notification> Notification{ get; set; }
        public DbSet<FriendShip> FriendShip { get; set; }
    }
}
