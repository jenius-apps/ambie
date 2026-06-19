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

    /// <summary>
    /// List of pages that the category works on.
    /// </summary>
    /// <remarks>
    /// Maps to <see cref="CategorySupportedPage"/> enum, but we use a string for simple deserialization.
    /// A null value means the category can work on all pages.
    /// </remarks>
    public IReadOnlyList<string>? SupportedPages { get; init; }
}

/// <summary>
/// List of pages that a category can support.
/// </summary>
public enum CategorySupportedPage
{
    /// <summary>
    /// Represents that the catalogue page is supported by the category.
    /// </summary>
    Catalogue,

    /// <summary>
    /// Represents that the channel page is supported by the category.
    /// </summary>
    Channel,

    /// <summary>
    /// Represents that the guide page is supported by the category.
    /// </summary>
    Guide,
}
