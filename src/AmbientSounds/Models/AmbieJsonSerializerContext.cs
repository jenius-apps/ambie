using System.Text.Json.Serialization;

namespace AmbientSounds.Models
{
    /// <summary>
    /// The <see cref="JsonSerializerContext"/> with type info for all JSON models used by Ambie.
    /// </summary>
    [JsonSerializable(typeof(FocusHistory))]
    [JsonSerializable(typeof(FocusHistorySummary))]
    public sealed partial class AmbieJsonSerializerContext : JsonSerializerContext
    {
    }
}
