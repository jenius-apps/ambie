using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AmbientSounds.Models;

public class Guide : IAsset
{
    /// <summary>
    /// GUID for the guide object.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Represents the length of the guide in minutes.
    /// </summary>
    public double MinutesLength { get; set; }

    /// <summary>
    /// List of semicolon-separated strings representing
    /// sound IDs that are suggested as background for this 
    /// meditation guide.
    /// </summary>
    /// <remarks>
    /// E.g. "[ 'soundIdA', 'soundIdB;soundIdC', 'soundIdX' ]"
    /// </remarks>
    public IReadOnlyList<string> SuggestedBackgroundSounds { get; set; } = Array.Empty<string>();

    /// <summary>
    /// User-facing name for this asset.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Localizations for this asset.
    /// </summary>
    public IReadOnlyDictionary<string, DisplayInformation> Localizations { get; set; } = new Dictionary<string, DisplayInformation>();

    /// <summary>
    /// Version of this data file.
    /// </summary>
    public int MetaDataVersion { get; set; }

    /// <summary>
    /// Version of the sound file itself.
    /// </summary>
    public int FileVersion { get; set; }

    /// <summary>
    /// Ids used to identify the IAPs
    /// associated with this sound.
    /// </summary>
    public IReadOnlyList<string> IapIds { get; set; } = Array.Empty<string>();

    /// <summary>
    /// True if guide is premium.
    /// </summary>
    public bool IsPremium { get; set; }

    /// <summary>
    /// Path to image file.
    /// </summary>
    public string ImagePath { get; set; } = "";

    /// <summary>
    /// True if the guide has been downloaded and is available
    /// for offline playback.
    /// </summary>
    /// <remarks>
    /// Ignored from serialization since it's only used as
    /// a flag during runtime.
    /// </remarks>
    [JsonIgnore]
    public bool IsDownloaded { get; set; }

    /// <summary>
    /// The download URL for the guide. This can
    /// be empty, particularly if the guide has been downloaded
    /// already.
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;

    /// <summary>
    /// Local path for the guide. This can be empty,
    /// particularly if the guide has not been downloaded
    /// yet.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Extension of sound file.
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    public override string ToString() => Name;
}

public record QueuedGuide(Guide Guide, IProgress<double> Progress);
