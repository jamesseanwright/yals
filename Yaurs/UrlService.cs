namespace Yaurs;

class UrlService(UrlDb urlDb) : IUrlService
{
    public Task<Url> GetUrlAsync(Ulid id)
    {
        throw new NotImplementedException();
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
