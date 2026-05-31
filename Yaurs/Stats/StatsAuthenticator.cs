using System.Security.Cryptography;
using System.Text;
using Yaurs.Urls;

namespace Yaurs.Stats;

class StatsAuthenticator() : IStatsAuthenticator
{
    public void Authenticate(Url url, string? statsKey)
    {
        if (string.IsNullOrEmpty(statsKey))
        {
            throw new StatsKeyAuthenticationException();
        }

        byte[] bStatsKey;

        try
        {
            bStatsKey = Convert.FromHexString(statsKey);
        }
        catch (FormatException)
        {
            throw new StatsKeyAuthenticationException();
        }

        var concatentated = new byte[bStatsKey.Length + url.StatsKeySalt.Length];

        Buffer.BlockCopy(bStatsKey, 0, concatentated, 0, bStatsKey.Length);
        Buffer.BlockCopy(url.StatsKeySalt, 0, concatentated, bStatsKey.Length, url.StatsKeySalt.Length);

        byte[] hashedKey;

        try
        {
            hashedKey = SHA256.HashData(concatentated);
        }
        catch (Exception e)
        {
            throw new StatsKeyHashException(e.Message);
        }

        if (hashedKey is null)
        {
            throw new StatsKeyHashException("Stats hashed key was not computed");
        }

        if (!CryptographicOperations.FixedTimeEquals(hashedKey, url.HashedStatsKey))
        {
            throw new StatsKeyAuthenticationException();
        }
    }
}
