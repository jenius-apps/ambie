using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IRecentFocusService
    {
        /// <summary>
        /// Adds a focus setting to recents list
        /// and saves it in storage.
        /// </summary>
        Task AddRecentAsync(int focusMinutes, int restMinutes, int repeats, DateTime? lastUsed = null);

        /// <summary>
        /// Returns list of recent focus settings
        /// from storage.
        /// </summary>
        Task<IReadOnlyList<RecentFocusSettings>> GetRecentAsync();
        
        /// <summary>
        /// Deletes the settings configuration from storage.
        /// </summary>
        /// <param name="settings">The settings object to delete.</param>
        Task RemoveRecentAsync(RecentFocusSettings settings);
    }
}