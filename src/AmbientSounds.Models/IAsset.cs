using System.Collections.Generic;

namespace AmbientSounds.Models;

/// <summary>
/// An interface for a downloadable asset from Ambie.
/// </summary>
public interface IAsset : IHasLocalizations
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
    /// A list of categories associated with the asset.
    /// </summary>
    IReadOnlyList<string> CategoryIds { get; init; }
}
