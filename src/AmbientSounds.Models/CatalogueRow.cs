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
    public IReadOnlyList<string>? CategoryIds { get; init; } = [];

    /// <summary>
    /// Unused property. This CatalogueRow class will eventually be deleted and replaced,
    /// but this property was added for temporary backwards compatibility for the IAsset interface.
    /// </summary>
    public string Id { get; set; } = string.Empty;
}
