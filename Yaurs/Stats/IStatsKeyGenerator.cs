namespace Yaurs.Stats;

record GeneratedStatsKey
{
    public string RawKey { get; private set; } // TODO: make fields required
    public byte[] HashedKey { get; private set; }

    public byte[] Salt { get; private set; }
}

interface IStatsKeyGenerator
{
    GeneratedStatsKey Generate();
}
