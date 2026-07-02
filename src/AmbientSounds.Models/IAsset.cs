namespace AmbientSounds.Models;

/// <summary>
/// An interface for a downloadable asset from Ambie.
/// </summary>
public interface IAsset : IHasLocalizations, IHasCategories
{
    /// <summary>
    /// Name of asset.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Description of asset.
    /// </summary>
    string Description { get; set; }
}
