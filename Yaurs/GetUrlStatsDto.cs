namespace Yaurs;

record Hits
{
    public required int Lifetime { get; set; }
}

record GetUrlStatsDto
{
    public required Hits Hits { get; set; }
}
