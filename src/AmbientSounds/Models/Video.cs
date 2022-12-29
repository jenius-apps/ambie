using System;
using System.Text.Json.Serialization;

namespace AmbientSounds.Models
{
    /// <summary>
    /// An object representing a video that can be downloaded
    /// and played by Ambie.
    /// </summary>
    public class Video
    {
        /// <summary>
        /// GUID to uniquely identify the video. Required by database.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// User-friendly name that can be used by a client.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// True if the video has been downloaded and is available
        /// for offline playback.
        /// </summary>
        /// <remarks>
        /// Ignored from serialization since it's only used as
        /// a flag during runtime.
        /// </remarks>
        [JsonIgnore]
        public bool IsDownloaded { get; set; }

        /// <summary>
        /// The download URL for the video. This can
        /// be empty, particularly if the video has been downloaded
        /// already.
        /// </summary>
        public string DownloadUrl { get; set; } = string.Empty;

        /// <summary>
        /// Local path for the video. This can be empty,
        /// particularly if the video has not been downloaded
        /// yet.
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// The extension portion of the file.
        /// E.g. ".mp4" or ".avi"
        /// </summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>
        /// Size of video file in MB.
        /// </summary>
        public double MegaByteSize { get; set; }

        /// <summary>
        /// True if video is premium.
        /// </summary>
        public bool IsPremium { get; set; }

        /// <summary>
        /// Ids used to identify the IAPs
        /// associated with this video.
        /// </summary>
        public string[] IapIds { get; set; } = Array.Empty<string>();
    }
}
