using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Star_Wars.Models;
using System.Text.Json;

namespace Star_Wars.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Starship> Starships { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure Starship entity
            builder.Entity<Starship>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.Manufacturer);
                entity.HasIndex(e => e.StarshipClass);
                entity.HasIndex(e => e.IsActive);

                // Simplified JSON configuration for list properties
                entity.Property(e => e.Pilots)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                        v => JsonSerializer.Deserialize<List<string>>(v, default(JsonSerializerOptions)) ?? new List<string>());

                entity.Property(e => e.Films)
                    .HasConversion(
                        v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                        v => JsonSerializer.Deserialize<List<string>>(v, default(JsonSerializerOptions)) ?? new List<string>());
            });

            // Configure ApplicationUser entity
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.ApiKey).IsUnique();
                
                entity.Property(e => e.ApiKey)
                    .HasMaxLength(255);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Starship && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                if (entityEntry.Entity is Starship starship)
                {
                    starship.UpdatedAt = DateTime.UtcNow;

                    if (entityEntry.State == EntityState.Added)
                    {
                        starship.CreatedAt = DateTime.UtcNow;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
