using Yals.Urls;

namespace Yals.Stats;

interface IStatsAuthenticator
{
    void Authenticate(Url url, string? statsKey);
}
