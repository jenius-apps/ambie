using AmbientSounds.Models;
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
        /// <returns>List of sounds tht have an update available.</returns>
        Task<IReadOnlyList<Sound>> CheckForUpdatesAsync(CancellationToken ct);
    }
}