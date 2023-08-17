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
    string GetLocalDescription(IAsset asset);

    /// <summary>
    /// Returns true if the given name query is contained inside the local name
    /// of the given asset.
    /// </summary>
    /// <param name="asset">The asset whose name to check.</param>
    /// <param name="nameQuery">The string name to search in the asset's name.</param>
    /// <returns>True if the name query is contained in the asset's name. False, otherwise.</returns>
    bool LocalNameContains(IAsset asset, string nameQuery);
}