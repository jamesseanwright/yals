namespace Yaurs;

class UrlService(UrlDb urlDb) : IUrlService
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

    public async Task<Url> CreateUrlAsync(Uri targetUri)
    {
        var id = Ulid.NewUlid();

        urlDb.Add(new UrlEntity
        {
            Id = id,
            TargetUri = targetUri,
        });

        await urlDb.SaveChangesAsync();

        return new Url
        {
            Id = id,
            TargetUri = targetUri,
        };
    }
}
