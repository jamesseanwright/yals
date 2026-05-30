using Yaurs.Stats;

namespace Yaurs;

record GetUrlDto : Url
{
    public required IList<Stat> Stats { get; set; }
}
