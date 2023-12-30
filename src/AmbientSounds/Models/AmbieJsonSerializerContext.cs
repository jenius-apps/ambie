using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AmbientSounds.Models;

/// <summary>
/// The <see cref="JsonSerializerContext"/> with type info for all JSON models used by Ambie.
/// </summary>
/// <remarks>
/// <para>
/// This type acts as the receiver for the System.Text.Json generator to generate all the JSON serialization
/// code for the types marked as annotations here. That is, instead of using runtime reflection, the generator
/// will generate all the necessary serialization code at build time, so that all the code can be trimmed and
/// also run faster at runtime. See: <see href="https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation"/>.
/// </para>
/// <para>
/// There are two types of annotations used here, primarily:
/// <list type="bullet">
///   <item>
///     Types that are only deserialized are marked with JsonSourceGenerationMode.Metadata. This is only a subset of the
///     code that's possible to generate, and it won't include the deserialization code, as that's not needed.  
///   </item>
///   <item>Types that are both serialized and deserialized just use the default setting, which include both modes.</item>
/// </list>
/// </para>
/// <para>
/// If you add a new type that needs to use JSON serialization, make sure to add an attribute here. Then to serialize:
/// <code>
/// Do NOT do this as before:
/// JsonSerializer.Serialize(myModel);
/// 
/// // Do THIS instead:
/// JsonSerializer.Serialize(myModel, AmbieJsonSerializerContext.Default.MyModel);
/// </code>
/// </para>
/// <para>
/// That is, pass the generated <see cref="System.Text.Json.Serialization.Metadata.JsonTypeInfo{T}"/> property that matches that model type. Same to deserialize.
/// </para>
/// </remarks>
[JsonSerializable(typeof(CatalogueRow))]
[JsonSerializable(typeof(FocusHistory))]
[JsonSerializable(typeof(StreakHistory))]
[JsonSerializable(typeof(ShareDetail))]
[JsonSerializable(typeof(FocusHistorySummary))]
[JsonSerializable(typeof(FocusTask[]), GenerationMode = JsonSourceGenerationMode.Metadata)] // Only used to deserialize
[JsonSerializable(typeof(IEnumerable<FocusTask>))]
[JsonSerializable(typeof(Video[]), GenerationMode = JsonSourceGenerationMode.Metadata)] // Only used to deserialize
[JsonSerializable(typeof(CatalogueRow[]), GenerationMode = JsonSourceGenerationMode.Metadata)] // Only used to deserialize
[JsonSerializable(typeof(IList<Video>))]
[JsonSerializable(typeof(SyncData))]
[JsonSerializable(typeof(Guide[]), GenerationMode = JsonSourceGenerationMode.Metadata)] // Only used to deserialize
[JsonSerializable(typeof(Sound[]), GenerationMode = JsonSourceGenerationMode.Metadata)] // Only used to deserialize
[JsonSerializable(typeof(IReadOnlyList<Guide>))]
[JsonSerializable(typeof(List<Sound>))]
[JsonSerializable(typeof(IList<Sound>))]
[JsonSerializable(typeof(RecentFocusSettings[]))]
public sealed partial class AmbieJsonSerializerContext : JsonSerializerContext
{
    /// <summary>
    /// The lazily initialized backing field for the context to be used for case insensitive serialization (<see cref="CaseInsensitive"/>).
    /// </summary>
    private static AmbieJsonSerializerContext? _caseInsensitive;

    /// <summary>
    /// A case insensitive variant of <see cref="Default"/>.
    /// </summary>
    public static AmbieJsonSerializerContext CaseInsensitive => _caseInsensitive ??= new AmbieJsonSerializerContext(new JsonSerializerOptions(s_defaultOptions) { PropertyNameCaseInsensitive = true });
}
