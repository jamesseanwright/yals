namespace Yals.Stats;

record GeneratedStatsKey
{
    public required string RawKey { get; set; }
    public required byte[] HashedKey { get; set; }

    public required byte[] Salt { get; set; }
}

interface IStatsKeyGenerator
{
    GeneratedStatsKey Generate();
}
