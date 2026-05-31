namespace Yaurs.Urls;

public record CreatedUrlDto
{
    public required Ulid Id { get; set; }
    public required Uri TargetUri { get; set; }
    public required string StatsKey { get; set; }
}
