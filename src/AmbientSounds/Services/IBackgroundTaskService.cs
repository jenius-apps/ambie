using System.Threading.Tasks;

namespace AmbientSounds.Services;

/// <summary>
/// Responsible for handling background task
/// registration and unregistration.
/// </summary>
public interface IBackgroundTaskService
{
    /// <summary>
    /// Unregisters all background tasks.
    /// </summary>
    void UnregisterAllTasks();

    /// <summary>
    /// Requests permission for background tasks to be used.
    /// </summary>
    /// <returns>True if permission is granted. False, otherwise.</returns>
    Task<bool> RequestPermissionAsync();

    /// <summary>
    /// Registers or unregisters the quick resume startup task.
    /// </summary>
    /// <param name="enable">Task will be registered if this is set to true. It will be disabled if false.</param>
    void ToggleQuickResumeStartupTask(bool enable);

    /// <summary>
    /// Registers or unregisteres the streak reminder task.
    /// </summary>
    /// <param name="enable">Task will be registered if this is set to true. It will be disabled if false.</param>
    void ToggleStreakReminderTask(bool enable);

    /// <summary>
    /// Unregisters the specific task.
    /// </summary>
    /// <param name="name">Name of task.</param>
    void UnregisterTask(string name);
}
