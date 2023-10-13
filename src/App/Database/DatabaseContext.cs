using App.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace App.Database;

internal sealed class DatabaseContext : DbContext
{
    private static bool _isMigrated;

    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
        if (_isMigrated)
        {
            return;
        }

        if (Database.IsRelational() && Database.GetPendingMigrations().Any())
        {
            Database.Migrate();
        }

        _isMigrated = true;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Entity>();
    }
}