using AmbientSounds.Models;

#nullable enable

namespace AmbientSounds.Services;

public interface IAssetLocalizer
{
    /// <summary>
    /// Extracts localized name from the given asset.
    /// </summary>
    /// <param name="asset">The asset whose name to extract.</param>
    /// <returns>Localized name of the asset.</returns>
    string GetLocalName(IAsset asset);

    /// <summary>
    /// Extracts localized description from the given asset.
    /// </summary>
    /// <param name="asset">The asset whose description to extract.</param>
    /// <returns>Localized description of the asset.</returns>
    string GetDescription(IAsset asset);
}