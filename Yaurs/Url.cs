namespace Yaurs;

public record Url
{
    public Ulid Id { get; set; }
    public required Uri TargetUri { get; set; }
}
