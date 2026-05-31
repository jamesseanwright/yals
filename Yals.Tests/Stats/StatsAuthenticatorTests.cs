namespace Yals.Tests.Stats;

using System.Security.Cryptography;
using Yals.Stats;
using Yals.Urls;

public class StatsAuthenticatorTests
{
    private readonly StatsAuthenticator authenticator = new();

    private static Url BuildUrlWithKey(byte[] rawKey, byte[] salt)
    {
        var concatenated = new byte[rawKey.Length + salt.Length];

        Buffer.BlockCopy(rawKey, 0, concatenated, 0, rawKey.Length);
        Buffer.BlockCopy(salt, 0, concatenated, rawKey.Length, salt.Length);

        return new Url
        {
            Id = Ulid.NewUlid(),
            TargetUri = new Uri("https://example.com"),
            HashedStatsKey = SHA256.HashData(concatenated),
            StatsKeySalt = salt,
        };
    }

    [Fact]
    public void Authenticate_Succeeds_WithMatchingKey()
    {
        var rawKey = RandomNumberGenerator.GetBytes(32);
        var salt = RandomNumberGenerator.GetBytes(16);
        var url = BuildUrlWithKey(rawKey, salt);

        authenticator.Authenticate(url, Convert.ToHexString(rawKey));
    }

    [Fact]
    public void Authenticate_Throws_WhenKeyIsWrong()
    {
        var rawKey = RandomNumberGenerator.GetBytes(32);
        var salt = RandomNumberGenerator.GetBytes(16);
        var url = BuildUrlWithKey(rawKey, salt);

        var wrongKey = RandomNumberGenerator.GetBytes(32);

        Assert.Throws<StatsKeyAuthenticationException>(
            () => authenticator.Authenticate(url, Convert.ToHexString(wrongKey)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Authenticate_Throws_WhenKeyIsNullOrEmpty(string? statsKey)
    {
        var url = BuildUrlWithKey(RandomNumberGenerator.GetBytes(32), RandomNumberGenerator.GetBytes(16));

        Assert.Throws<StatsKeyAuthenticationException>(
            () => authenticator.Authenticate(url, statsKey));
    }
}
