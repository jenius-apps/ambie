using AmbientSounds.Models;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

/// <summary>
/// Interface for streak history cache.
/// </summary>
public interface IStreakHistoryCache
{
    /// <summary>
    /// Retrieves streak history from cache.
    /// </summary>
    /// <returns>Streak history.</returns>
    Task<StreakHistory> GetStreakHistoryAsync();

    /// <summary>
    /// Updates the stored streak history with the given object.
    /// </summary>
    /// <param name="updatedHistory">The streak history with updated information.</param>
    Task UpdateStreakHistory(StreakHistory updatedHistory);
}