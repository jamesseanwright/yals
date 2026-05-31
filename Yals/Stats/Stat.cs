namespace Yals.Stats;

record Stat
{
    public required StatType Type { get; set; }

    public int Hits { get; set; }
}
