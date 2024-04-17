using System;

namespace AmbientSounds.Events;

/// <summary>
/// Event arguments class representing a video download that was triggered.
/// </summary>
public sealed class VideoDownloadTriggeredArgs : EventArgs
{
    /// <summary>
    /// The ID of the video being downloaded.
    /// </summary>
    public required string VideoId { get; init; }

    /// <summary>
    /// The sound ID associated with the video.
    /// </summary>
    public required string SoundId { get; init; }

    /// <summary>
    /// Tracks the progress of the download, from 0-100.
    /// </summary>
    public required Progress<double> Progress { get; init; }
}
