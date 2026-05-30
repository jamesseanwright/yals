namespace Yaurs.Stats;

interface IStatsAuthenticator
{
    void Authenticate(Url url, string? statsKey);
}
