namespace Yaurs;

using Microsoft.EntityFrameworkCore;

public class UrlDb(DbContextOptions<UrlDb> options) : DbContext(options)
{
    public DbSet<UrlEntity> Urls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TODO: see if this can be applied to the type across the project
        modelBuilder.Entity<UrlEntity>()
            .Property(e => e.Id)
            .HasConversion(
                v => v.ToGuid(),
                v => new Ulid(v)
            );
    }
}