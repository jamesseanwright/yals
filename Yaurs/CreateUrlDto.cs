namespace Yaurs;

public record CreateUrlDto
{
    public required Uri TargetUri { get; set; }
}