using System.Collections.Generic;

namespace AmbientSounds.Models;

/// <summary>
/// User's streak and overall usage history.
/// </summary>
public class StreakHistory
{
    /// <summary>
    /// The dates when the user was active.
    /// </summary>
    /// <remarks>
    /// Example: Years["2023"]["12"] = [ 26, 27 ] => User was active on December 26 and 27 of 2023.
    /// </remarks>
    public Dictionary<string, Dictionary<string, HashSet<int>>> Years { get; set; } = [];

    /// <summary>
    /// Longest consecutive day streak.
    /// </summary>
    public int LongestStreak { get; set; }

    /// <summary>
    /// Total hours of usage.
    /// </summary>
    public double TotalHours { get; set; }

    /// <summary>
    /// An array of 12 doubles. Each position represents a month of the year, January = 0, February = 1, etc.
    /// The value in each position represents the usage hours for that month.
    /// </summary>
    public double[] MonthlyHours { get; set; } = new double[12];

    /// <summary>
    /// An array of 7 doubles. Each position represents a day of the week, Sunday = 0, Monday = 1, etc.
    /// The value in each position represents the usage hours for that day.
    /// </summary>
    public double[] WeeklyHours { get; set; } = new double[7];

    /// <summary>
    /// Total hours of focus usage.
    /// </summary>
    public double TotalFocusHours { get; set; }

    /// <summary>
    /// Total amount of tasks completed during a focus session.
    /// </summary>
    public int TotalTasksCompleted { get; set; }
}
