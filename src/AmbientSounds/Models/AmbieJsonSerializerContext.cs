using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AmbientSounds.Models
{
    /// <summary>
    /// The <see cref="JsonSerializerContext"/> with type info for all JSON models used by Ambie.
    /// </summary>
    [JsonSerializable(typeof(FocusHistory))]
    [JsonSerializable(typeof(FocusHistorySummary))]
    [JsonSerializable(typeof(FocusTask[]), GenerationMode = JsonSourceGenerationMode.Metadata)] // Only used to deserialize
    [JsonSerializable(typeof(IEnumerable<FocusTask>))]
    [JsonSerializable(typeof(Video[]), GenerationMode = JsonSourceGenerationMode.Metadata)] // Only used to deserialize
    [JsonSerializable(typeof(IList<Video>))]
    [JsonSerializable(typeof(SyncData))]
    [JsonSerializable(typeof(Sound[]), GenerationMode = JsonSourceGenerationMode.Metadata)] // Only used to deserialize
    [JsonSerializable(typeof(List<Sound>))]
    [JsonSerializable(typeof(RecentFocusSettings[]))]
    public sealed partial class AmbieJsonSerializerContext : JsonSerializerContext
    {
    }
}
