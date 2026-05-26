namespace Yaurs;

interface IUrlService
{
    Task<Url> CreateUrlAsync(Uri targetUri);
}
