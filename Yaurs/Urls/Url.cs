namespace Yaurs.Urls;

public record Url
{
    public required Ulid Id { get; set; }
    public required Uri TargetUri { get; set; }

    public required byte[] HashedStatsKey { get; set; }

    public required byte[] StatsKeySalt { get; set; }

    public int LifetimeHits { get; set; } = 0;
}
