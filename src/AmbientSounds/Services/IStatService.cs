using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

/// <summary>
/// Service to retrieve and manipulate the user's stats.
/// </summary>
public interface IStatService
{
    /// <summary>
    /// Raised when the streak data changes.
    /// </summary>
    event EventHandler<StreakChangedEventArgs>? StreakChanged;

    /// <summary>
    /// Retrieves the current active streak.
    /// </summary>
    /// <remarks>
    /// If it's determined that the streak was broken, then returns 0.
    /// </remarks>
    /// <returns>Integer representing the number of days for the current active streak.</returns>
    int ValidateAndRetrieveStreak();

    /// <summary>
    /// Log activity for today and extend the streak.
    /// </summary>
    Task LogStreakAsync();

    /// <summary>
    /// Retreives recent active history days.
    /// </summary>
    /// <param name="days">The number of days to retrieve.</param>
    /// <returns>
    /// List of boolean. Each bool represents one of the last given days.
    /// True means the user was active on that day.
    /// </returns>
    Task<IReadOnlyList<bool>> GetRecentActiveHistory(int days);

    /// <summary>
    /// Retrieves streak history object.
    /// </summary>
    /// <returns>The streak history object.</returns>
    Task<StreakHistory> GetStreakHistory();
}