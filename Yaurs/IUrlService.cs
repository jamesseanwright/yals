namespace Yaurs;

interface IUrlService
{
    Task<Url?> GetUrlAsync(Ulid id);
    Task<CreatedUrlDto> CreateUrlAsync(Uri targetUri);
}
