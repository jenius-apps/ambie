using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AmbientSounds.Models;

/// <summary>
/// Represents a channel asset that can play content at full screen.
/// </summary>
/// <remarks>
/// A channel inherently does not directly represent a video or sound asset.
/// Instead, a channel is a collection of videos and sounds that is played simultaneously. 
/// Thus, a channel itself shouldn't be downloaded.
/// </remarks>
public class Channel : IAsset
{
    /// <summary>
    /// Id for this channel.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// English non-localized name for this channel.
    /// </summary>
    /// <remarks>
    /// Should only be used for internal purposes such as telemetry. 
    /// Do not display on UI since it's not localized.
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// English non-localized description for this channel.
    /// </summary>
    /// <remarks>
    /// Should only be used for internal purposes such as telemetry. 
    /// Do not display on UI since it's not localized.
    /// </remarks>
    public string Description { get; set; } = string.Empty;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, DisplayInformation> Localizations { get; set; } = new Dictionary<string, DisplayInformation>();

    /// <summary>
    /// List of videos to be displayed by this channel.
    /// </summary>
    public IReadOnlyList<string> VideoIds { get; set; } = [];

    /// <summary>
    /// List of sounds to be played by this channel.
    /// </summary>
    public IReadOnlyList<string> SoundIds { get; set; } = [];

    /// <summary>
    /// Path to image file.
    /// </summary>
    public string ImagePath { get; set; } = string.Empty;

    /// <summary>
    /// Colour that can be used to decorate the channel in the UI.
    /// </summary>
    public string ColourHex { get; set; } = string.Empty;

    /// <summary>
    /// Ids used to identify the IAPs
    /// associated with this asset.
    /// </summary>
    public IReadOnlyList<string> IapIds { get; set; } = [];

    /// <summary>
    /// Describes the type of content to be displayed.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<ChannelType>))]
    public ChannelType Type { get; set; }
}

/// <summary>
/// Describes the type of content that will be displayed by the <see cref="Channel"/>.
/// </summary>
public enum ChannelType
{
    /// <summary>
    /// The default channel that uses videos. 
    /// </summary>
    /// <remarks>
    /// The metadata in <see cref="Channel"/> is associated with the this type.
    /// </remarks>
    Videos,

    /// <summary>
    /// An override that will make the channel open a dark screen with no video or images.
    /// </summary>
    DarkScreen,

    /// <summary>
    /// An override that will make the channel open a slideshow of images based on the currently playing sounds.
    /// </summary>
    /// <remarks>
    /// The logic for the slideshow is local and has nothing to do with the metadata in <see cref="Channel"/>.
    /// </remarks>
    Slideshow,
}