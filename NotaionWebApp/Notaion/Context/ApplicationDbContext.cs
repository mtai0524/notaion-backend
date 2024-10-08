using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notaion.Entities;
using Notaion.Models;

namespace Notaion.Context
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
