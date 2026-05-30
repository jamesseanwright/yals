namespace Yaurs;

interface IUrlService
{
    Task<GetUrlDto?> GetUrlAsync(Ulid id);
    Task<CreatedUrlDto> CreateUrlAsync(Uri targetUri);
    Task RegisterHit(Ulid Id);
}
