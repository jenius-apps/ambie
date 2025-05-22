using System.Threading.Tasks;

namespace AmbientSounds.Services;

/// <summary>
/// Service for enabling streak reminders.
/// </summary>
public interface IStreakReminderService
{
    /// <summary>
    /// Disables the streak reminder.
    /// </summary>
    void Disable();

    /// <summary>
    /// Attempts to enable the streak reminder.
    /// </summary>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> TryEnableAsync();
}