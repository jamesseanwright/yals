namespace Yals.Tests.Stats;

using Yals.Crypto;
using Yals.Stats;
using Yals.Urls;

public class StatsKeyGeneratorTests
{
    private readonly StatsKeyGenerator generator = new(new CryptoRandomNumberGenerator());

    [Fact]
    public void Generate_ProducesExpectedDimensions()
    {
        var result = generator.Generate();

        Assert.Equal(64, result.RawKey.Length); // 32 bytes hex-encoded = 64 chars
        Assert.Equal(16, result.Salt.Length);
        Assert.Equal(32, result.HashedKey.Length); // SHA256 output = 32 bytes
    }

    [Fact]
    public void Generate_ProducesDifferentKeysOnEachCall()
    {
        var first = generator.Generate();
        var second = generator.Generate();

        Assert.NotEqual(first.RawKey, second.RawKey);
    }

    [Fact]
    public void Generate_IntegratesWithStatsAuthenticator()
    {
        var generated = generator.Generate();
        var url = new Url
        {
            Id = Ulid.NewUlid(),
            TargetUri = new Uri("https://example.com"),
            HashedStatsKey = generated.HashedKey,
            StatsKeySalt = generated.Salt,
        };

        new StatsAuthenticator().Authenticate(url, generated.RawKey);
    }
}
