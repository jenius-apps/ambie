using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services;

public interface IShareService
{
    /// <summary>
    /// List of failed sound IDs.
    /// </summary>
    /// <remarks>
    /// Populated when <see cref="ShareFailed"/>
    /// is fired.
    /// </remarks>
    IReadOnlyList<string>? FailedSoundIds { get; }

    /// <summary>
    /// Raised after a share was received and processed.
    /// Payload is the list of sounds that was received.
    /// </summary>
    /// <remarks>
    /// The intention of this event is for the UI layer to
    /// acknowledge the share and attempt to play the sounds.
    /// </remarks>
    event EventHandler<IReadOnlyList<string>>? ShareRequested;

    /// <summary>
    /// Raised when the share playback has been attempted
    /// but one or more sounds were missing, so the share did
    /// have 100% playback success.
    /// </summary>
    event EventHandler? ShareFailed;

    /// <summary>
    /// The initial method that receives a share request
    /// from outside the application and processes it.
    /// After processing, <see cref="ShareRequested"/>
    /// is raised.
    /// </summary>
    /// <param name="shareId">The share ID to process.</param>
    Task ProcessShareRequestAsync(string shareId);

    /// <summary>
    /// Retrieves a share URL for the given list of sound IDs.
    /// </summary>
    /// <param name="soundIds">List of sounds to share.</param>
    /// <returns>A URL string that can be shared with other users.</returns>
    Task<string> GetShareUrlAsync(IReadOnlyList<string> soundIds);

    /// <summary>
    /// Method to track playback failure of the share. 
    /// This will raise <see cref="ShareFailed"/>.
    /// </summary>
    /// <param name="soundIds">
    /// Full list of shared sound IDs. One of more of these failed to play.
    /// </param>
    void LogShareFailed(IReadOnlyList<string> soundIds);
}