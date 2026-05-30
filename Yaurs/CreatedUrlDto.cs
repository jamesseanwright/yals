using Yaurs.Stats;

namespace Yaurs;

public record CreatedUrlDto : Url
{
    public required string StatsKey { get; set; }
}
