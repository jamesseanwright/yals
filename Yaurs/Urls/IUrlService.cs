namespace Yaurs.Urls;

interface IUrlService
{
    Task<Url?> GetUrlAsync(Ulid id);
    Task<CreatedUrlDto> CreateUrlAsync(Uri targetUri);
    Task RegisterHit(Ulid Id);
}
