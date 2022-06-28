using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Cache
{
    public interface IFocusHistoryCache
    {
        Task<IReadOnlyList<FocusHistory>> GetRecentAsync();

        /// <summary>
        /// Adds the new history to cache and storage.
        /// </summary>
        Task AddHistoryAsync(FocusHistory focusHistory);
    }
}
