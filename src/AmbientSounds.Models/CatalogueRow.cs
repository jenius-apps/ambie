using System.Collections.Generic;

namespace AmbientSounds.Models;

public class CatalogueRow : IAsset
{
    /// <inheritdoc/>
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The sound IDs in the row.
    /// </summary>
    public IReadOnlyList<string> SoundIds { get; set; } = [];

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, DisplayInformation> Localizations { get; set; } = new Dictionary<string, DisplayInformation>();

    /// <inheritdoc/>
    /// <inheritdoc/>
    public IReadOnlyList<string>? CategoryIds { get; init; } = [];
}
