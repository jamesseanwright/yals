namespace Yaurs;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class UrlDb(DbContextOptions<UrlDb> options) : DbContext(options)
{
    public DbSet<UrlEntity> Urls { get; set; }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.Properties<Ulid>().HaveConversion<UlidToGuidConverter>();
    }
}

class UlidToGuidConverter() : ValueConverter<Ulid, Guid>(v => v.ToGuid(), v => new Ulid(v));