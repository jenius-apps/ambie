using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AmbientSounds.Models;

/// <summary>
/// Represents a category that can be attached to assets.
/// This can be used for filtering purposes of those assets.
/// </summary>
public class Category : IHasLocalizations
{
    /// <summary>
    /// The ID of the category. The intent is for this to be a lowercase, english word, such as
    /// 'lofi' or 'water'.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, DisplayInformation> Localizations { get; set; } = new Dictionary<string, DisplayInformation>();
}
