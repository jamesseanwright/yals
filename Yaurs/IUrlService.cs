namespace Yaurs;

interface IUrlService
{
    Task<Url> GetUrlAsync(Ulid id);
    Task<Url> CreateUrlAsync(Uri targetUri);
}
