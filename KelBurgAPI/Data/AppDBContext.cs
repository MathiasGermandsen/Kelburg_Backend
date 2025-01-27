using KelBurgAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace KelBurgAPI.Data;

public class AppDBContext : DbContext
{
    public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Common &&
                        (e.State == EntityState.Added 
                         || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Common)entry.Entity;

            if (entry.State == EntityState.Added)
            {
                if (String.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = Guid.NewGuid().ToString();
                }
                entity.CreatedAt = DateTime.UtcNow;
            }
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }   
}