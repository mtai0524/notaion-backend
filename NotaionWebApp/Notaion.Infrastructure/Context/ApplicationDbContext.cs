using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Notaion.Domain.Entities;
using Notaion.Domain.Models;


namespace Notaion.Infrastructure.Context
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Store DailyNote.Attachments (a typed list) as a JSON string column.
            // The controller upserts via CurrentValues.SetValues, which only copies
            // scalar properties — a value-converted property counts as scalar, so
            // edits to attachments persist (an OwnsMany/ToJson mapping would not).
            var attachmentsConverter = new ValueConverter<List<Attachment>, string>(
                v => JsonSerializer.Serialize(v ?? new List<Attachment>(), (JsonSerializerOptions?)null),
                v => string.IsNullOrEmpty(v)
                    ? new List<Attachment>()
                    : (JsonSerializer.Deserialize<List<Attachment>>(v, (JsonSerializerOptions?)null) ?? new List<Attachment>()));

            var attachmentsComparer = new ValueComparer<List<Attachment>>(
                (a, b) => JsonSerializer.Serialize(a, (JsonSerializerOptions?)null) == JsonSerializer.Serialize(b, (JsonSerializerOptions?)null),
                v => v == null ? 0 : JsonSerializer.Serialize(v, (JsonSerializerOptions?)null).GetHashCode(),
                v => (JsonSerializer.Deserialize<List<Attachment>>(JsonSerializer.Serialize(v, (JsonSerializerOptions?)null), (JsonSerializerOptions?)null) ?? new List<Attachment>()));

            builder.Entity<DailyNote>()
                .Property(e => e.Attachments)
                .HasConversion(attachmentsConverter)
                .Metadata.SetValueComparer(attachmentsComparer);
        }

        public DbSet<Item> Items { get; set; }
        public DbSet<User> User { get; set; }
        public DbSet<Page> Page { get; set; }
        public DbSet<Chat> Chat { get; set; }
        public DbSet<ChatPrivate> ChatPrivate { get; set; }
        public DbSet<Notification> Notification{ get; set; }
        public DbSet<FriendShip> FriendShip { get; set; }
        public DbSet<DailyNote> DailyNotes { get; set; }
        public DbSet<PageVisit> PageVisits { get; set; }
        public DbSet<AIMemory> AIMemories { get; set; }
        public DbSet<FileMetadata> FileMetadatas { get; set; }
    }
}
