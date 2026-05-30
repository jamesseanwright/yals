using Microsoft.EntityFrameworkCore;
using Yaurs.Stats;

namespace Yaurs;

class UrlService(UrlDb urlDb, IStatsKeyGenerator statsKeyGenerator) : IUrlService
{
    public async Task<Url?> GetUrlAsync(Ulid id)
    {
        var urlEntity = await urlDb.Urls.FindAsync(id);

        return urlEntity is not null ? new Url
        {
            Id = urlEntity.Id,
            TargetUri = urlEntity.TargetUri,
            HashedStatsKey = urlEntity.HashedStatsKey,
            StatsKeySalt = urlEntity.StatsKeySalt,
            LifetimeHits = urlEntity.LifetimeHits,
        } : null;
    }

    public async Task<CreatedUrlDto> CreateUrlAsync(Uri targetUri)
    {
        var id = Ulid.NewUlid();
        var statsKey = statsKeyGenerator.Generate();

        urlDb.Add(new UrlEntity
        {
            Id = id,
            TargetUri = targetUri,
            HashedStatsKey = statsKey.HashedKey,
            StatsKeySalt = statsKey.Salt,
        });

        await urlDb.SaveChangesAsync();

        return new CreatedUrlDto
        {
            Id = id,
            TargetUri = targetUri,
            StatsKey = statsKey.RawKey,
        };
    }

    public async Task RegisterHit(Ulid id)
    {
        // We're bypassing the EF Core change tracker here, but
        // given we won't typically depend upon the updated hit count
        // subsequently in the request lifecycle, we can optimise for performance.
        await urlDb.Urls
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(
            setters => setters.SetProperty(
                u => u.LifetimeHits,
                u => u.LifetimeHits + 1
            )
        );
    }
}
