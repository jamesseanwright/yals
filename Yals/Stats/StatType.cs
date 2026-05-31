using System.Text.Json.Serialization;

namespace Yals.Stats;

[JsonConverter(typeof(JsonStringEnumConverter<StatType>))]
enum StatType
{
    Lifetime,
}
