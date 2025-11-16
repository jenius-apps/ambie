using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class QuickResumeService : IQuickResumeService
{
    private readonly IBackgroundTaskService _backgroundTaskService;

    public QuickResumeService(IBackgroundTaskService backgroundTaskService)
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

        _backgroundTaskService.ToggleQuickResumeStartupTask(true);
        return true;
    }

    /// <inheritdoc/>
    public void Disable()
    {
        _backgroundTaskService.ToggleQuickResumeStartupTask(false);
    }
}
