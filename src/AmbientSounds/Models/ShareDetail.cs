#nullable enable

namespace AmbientSounds.Models;

/// <summary>
/// Data transfer object representing details
/// for sharing sounds.
/// </summary>
public class ShareDetail
{
    /// <summary>
    /// Id representing these details.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Semicolon separated list of sounds IDs. 
    /// </summary>
    public string SoundIdComposite { get; set; } = "";

    /// <summary>
    /// The link that clients can display in-app.
    /// E.g. https://ambieapp.com/share?code=12345.
    /// </summary>
    public string Link { get; set; } = "";
}
