namespace Yaurs.Stats;

record Stat
{
    public required string Type { get; set; }

    public int Hits { get; set; }
}
