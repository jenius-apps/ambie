using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AmbientSounds.Models;

/// <summary>
/// Represents a row of assets, such as sounds, channels, or guides.
/// be deserialized or used.
/// </summary>
public class AssetRow : IHasLocalizations
{
    /// <summary>
    /// Id of this asset row. Intended to be a human readable string that unique separates this
    /// asset row from other rows, but it is not intended to be displayed on UI.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// The type of asset this row contains.
    /// Based from <see cref="AssetRowType"/> but using
    /// string for easy serialization.
    /// </summary>
    public string? AssetType { get; init; }

    /// <summary>
    /// List of asset IDs.
    /// </summary>
    public IReadOnlyList<string> Assets { get; init; } = [];

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, DisplayInformation> Localizations { get; set; } = new Dictionary<string, DisplayInformation>();
}

/// <summary>
/// Represents the type of asset
/// the <see cref="AssetRow"/> contains.
/// </summary>
public enum AssetRowType
{
    /// <summary>
    /// The row contains a list of sounds.
    /// </summary>
    Sound,

    /// <summary>
    /// The row contains a list of channels.
    /// </summary>
    Channel,

    /// <summary>
    /// The row contains a list of guides.
    /// </summary>
    Guide
}
