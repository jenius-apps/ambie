namespace AmbientSounds.Models;

/// <summary>
/// An interface for a downloadable asset from Ambie.
/// </summary>
public interface IAsset : IHasLocalizations, IHasCategories
{
    /// <summary>
    /// A unique ID representing the asset.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// Name of asset.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Description of asset.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// The UTC datetime when the asset was published,
    /// represented in ticks.
    /// </summary>
    long PublishDateUtcTicks { get; init; }
}
