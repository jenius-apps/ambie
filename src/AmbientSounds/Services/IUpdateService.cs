using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IUpdateService
    {
        /// <summary>
        /// Checks if there are updates
        /// available for any installed sounds.
        /// </summary>
        /// <param name="ct">Cancellation token</param>
        /// <returns>List of sounds tht have an update available and the associated reason.</returns>
        Task<IReadOnlyList<(IVersionedAsset, UpdateReason)>> CheckForUpdatesAsync(CancellationToken ct);

        /// <summary>
        /// Attempts to trigger an update on the given sound data.
        /// </summary>
        /// <param name="latestSoundData">The up-to-date sound data.</param>
        /// <param name="progress">Download progress tracker object.</param>
        Task TriggerUpdateAsync(Sound latestSoundData, IProgress<double> progress);

        Task TriggerUpdateAsync(IVersionedAsset asset, IProgress<double> progress);
    }
}