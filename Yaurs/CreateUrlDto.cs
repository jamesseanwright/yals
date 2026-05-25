namespace Yaurs;

public record CreateUrlDto
{
    public required Uri TargetUrl { get; set; }
}