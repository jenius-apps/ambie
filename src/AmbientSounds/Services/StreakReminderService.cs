using System.Threading.Tasks;

namespace AmbientSounds.Services;

public sealed class StreakReminderService : IStreakReminderService
{
    private readonly IBackgroundTaskService _backgroundTaskService;

    public StreakReminderService(IBackgroundTaskService backgroundTaskService)
    {
        _backgroundTaskService = backgroundTaskService;
    }

    /// <inheritdoc/>
    public async Task<bool> TryEnableAsync()
    {
        var allowed = await _backgroundTaskService.RequestPermissionAsync();
        if (!allowed)
        {
            return false;
        }

        _backgroundTaskService.ToggleStreakReminderTask(true);
        return true;
    }

    /// <inheritdoc/>
    public void Disable()
    {
        _backgroundTaskService.ToggleStreakReminderTask(false);
    }
}
