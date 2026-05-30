using Microsoft.EntityFrameworkCore;
using Yaurs.Stats;

namespace Yaurs;

class UrlService(UrlDb urlDb, IStatsKeyGenerator statsKeyGenerator) : IUrlService
{
    public async Task<GetUrlDto?> GetUrlAsync(Ulid id)
    {
        var urlEntity = await urlDb.Urls.FindAsync(id);

        return urlEntity is not null ? new GetUrlDto
        {
            Id = urlEntity.Id,
            TargetUri = urlEntity.TargetUri,

            // Note that we surface hit statistics as a
            // list so that:
            //
            // 1. we can easily introduce new stat types in the future
            // 2. we can return a list of stats from our /urls/{id}/stats
            //    subresource, better following RESTful conventions.
            Stats = new List<Stat>([
                new Stat {
                    Type = StatType.Lifetime,
                    Hits = urlEntity.LifetimeHits,
                }
            ])
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
