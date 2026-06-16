using System.Collections.Generic;

namespace AmbientSounds.Models;

/// <summary>
/// An interface for a downloadable asset from Ambie.
/// </summary>
public interface IAsset
{
    /// <summary>
    /// Name of asset.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Description of asset.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Localizations for this asset.
    /// </summary>
    IReadOnlyDictionary<string, DisplayInformation> Localizations { get; set; }

    /// <summary>
    /// A list of categories associated with the asset.
    /// </summary>
    IReadOnlyList<string> Categories { get; set; }
}
