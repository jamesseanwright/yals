namespace Yaurs;

interface IUrlService
{
    Task<Url> GetUrlAsync(Ulid id);
}