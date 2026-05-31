namespace Yaurs.Urls;

public record CreateUrlDto
{
    public required Uri TargetUri { get; set; }
}
