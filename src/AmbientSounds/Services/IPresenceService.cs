using System;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Service used to track presence for sounds
    /// and reports when presence counts changed remotely.
    /// </summary>
    public interface IPresenceService
    {
        /// <summary>
        /// Raised when the sound presence counts change.
        /// </summary>
        event EventHandler<PresenceEventArgs>? SoundPresenceChanged;

        /// <summary>
        /// Raised when the presence connection is disconnected.
        /// </summary>
        event EventHandler? PresenceDisconnected;

        /// <summary>
        /// Informs server that the count should decrement for the given soundId.
        /// </summary>
        Task DecrementAsync(string soundId);

        /// <summary>
        /// Informs server that the count should increment for the given soundId.
        /// </summary>
        Task IncrementAsync(string soundId);

        /// <summary>
        /// Initializes the service and prepares to send/receive
        /// presence data to the server. Required before using <see cref="DecrementAsync"/>
        /// and <see cref="IncrementAsync"/>.
        /// </summary>
        Task EnsureInitializedAsync();

        /// <summary>
        /// Disconnects connection to the server.
        /// </summary>
        Task DisconnectAsync();
    }
}