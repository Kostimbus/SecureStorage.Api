using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecureStorage.Core.Models;

namespace SecureStorage.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<FileRecord> FileRecords { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // FileRecord mapping
            var file = modelBuilder.Entity<FileRecord>();
            file.ToTable("FileRecords");
            file.HasKey(f => f.Id);
            file.Property(f => f.FileName).IsRequired().HasMaxLength(512);
            file.Property(f => f.ContentType).IsRequired().HasMaxLength(128);
            file.Property(f => f.OwnerId).IsRequired();
            file.Property(f => f.EncryptedData).IsRequired();
            file.Property(f => f.PlaintextSize).IsRequired();
            file.Property(f => f.CreatedAtUtc).IsRequired();
            file.Property(f => f.UpdatedAtUtc).IsRequired(false);
            file.Property(f => f.Description).HasMaxLength(2000).IsRequired(false);

            // Indexes for queries
            file.HasIndex(f => f.OwnerId);

            // User mapping
            var user = modelBuilder.Entity<User>();
            user.ToTable("Users");
            user.HasKey(u => u.Id);
            user.Property(u => u.Username).IsRequired().HasMaxLength(256);
            user.Property(u => u.Email).IsRequired().HasMaxLength(320);
            user.Property(u => u.PasswordHash).HasMaxLength(1024).IsRequired(false);
            user.Property(u => u.CreatedAtUtc).IsRequired();
            user.Property(u => u.UpdatedAtUtc).IsRequired(false);
            user.Property(u => u.IsDisabled).IsRequired();

            // Unique constraints (make sure you enforce at app level too)
            user.HasIndex(u => u.Username).IsUnique();
            user.HasIndex(u => u.Email).IsUnique();
        }

        /// <summary>
        /// Automatically updates UpdatedAtUtc for modified FileRecord/User entities.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var utcNow = DateTime.UtcNow;

            // For FileRecord and User, set UpdatedAtUtc on modified entries
            foreach (var entry in ChangeTracker.Entries()
                       .Where(e => e.State == EntityState.Modified))
            {
                if (entry.Entity is FileRecord fr)
                {
                    fr.UpdatedAtUtc = utcNow;
                }
                else if (entry.Entity is User u)
                {
                    u.UpdatedAtUtc = utcNow;
                }
            }

            return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
