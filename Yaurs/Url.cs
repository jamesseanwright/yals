namespace Yaurs;

public record Url
{
    public required Ulid Id { get; set; }
    public required Uri TargetUri { get; set; }
}
