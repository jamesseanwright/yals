namespace Yals.Urls;

public record CreateUrlDto
{
    public required Uri TargetUri { get; set; }
}
