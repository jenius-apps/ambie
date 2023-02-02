﻿using System;
using System.Collections.Generic;

namespace AmbientSounds.Models;

public record QueuedSound(Sound SoundData, IProgress<double> Progress);

/// <summary>
/// A sound object.
/// </summary>
public class Sound : IAsset
{
    /// <summary>
    /// GUID.
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Path to image file.
    /// </summary>
    public string ImagePath { get; set; } = "";

    /// <summary>
    /// Name of sound.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Path of sound file.
    /// </summary>
    public string FilePath { get; set; } = "";

    /// <summary>
    /// Path of preview sound file.
    /// </summary>
    public string PreviewFilePath { get; set; } = "";

    /// <summary>
    /// Extension of sound file.
    /// </summary>
    public string FileExtension { get; set; } = "";

    /// <summary>
    /// Copyright or attribution text.
    /// </summary>
    public string Attribution { get; set; } = "";

    /// <summary>
    /// The list of image paths to be used
    /// for the screensaver.
    /// </summary>
    public string[]? ScreensaverImagePaths { get; set; }

    /// <summary>
    /// True if sound is premium.
    /// </summary>
    public bool IsPremium { get; set; }

    /// <summary>
    /// Id used to identify the IAP
    /// associated with this sound.
    /// </summary>
    [Obsolete("Use IapIds instead")]
    public string IapId { get; set; } = "";

    /// <summary>
    /// Ids used to identify the IAPs
    /// associated with this sound.
    /// </summary>
    public IReadOnlyList<string> IapIds { get; set; } = Array.Empty<string>();

    /// <summary>
    /// If true, this sound is a custom mix.
    /// </summary>
    public bool IsMix { get; set; }

    /// <summary>
    /// The list of image paths to be used
    /// for the mix.
    /// </summary>
    public string[] ImagePaths { get; set; } = Array.Empty<string>();

    /// <summary>
    /// List of sound Ids for this mix.
    /// </summary>
    public string[] SoundIds { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Unique user ID of the person who uploaded the sound file.
    /// </summary>
    public string UploadedBy { get; set; } = "";

    /// <summary>
    /// Visible username of the person who uploaded the sound file.
    /// </summary>
    public string UploadUsername { get; set; } = "";

    /// <summary>
    /// List of donation links to be displayed.
    /// </summary>
    public string[] SponsorLinks { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The state of the sound's publication in the catalogue.
    /// This is the string version of PublishState enum.
    /// </summary>
    public string PublishState { get; set; } = "";

    /// <summary>
    /// Colour to use to decorate the sound images.
    /// Supports RGB and ARGB.
    /// </summary>
    public string ColourHex { get; set; } = string.Empty;

    /// <summary>
    /// Used for sorting installed sounds.
    /// </summary>
    public int SortPosition { get; set; }

    /// <summary>
    /// Localizations for this sound.
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

    /// <inheritdoc/>
    public override string ToString()
    {
        return Name;
    }
}

public enum PublishState
{
    None,
    UnderReview,
    Published,
    Unpublished,
    Rejected
}
