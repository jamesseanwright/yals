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
}
