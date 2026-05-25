namespace Yaurs;

using Microsoft.EntityFrameworkCore;

class UrlDb(DbContextOptions<UrlDb> options) : DbContext(options)
{
    public DbSet<Url> Urls { get; set; }
}