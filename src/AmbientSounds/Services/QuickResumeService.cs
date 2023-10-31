using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services.Uwp;

public class QuickResumeService : IQuickResumeService
{
    private readonly IToastService _toastService;
    private readonly IBackgroundTaskService _backgroundTaskService;

    public QuickResumeService(
        IToastService toastService,
        IBackgroundTaskService backgroundTaskService)
    {
        _toastService = toastService;
        _backgroundTaskService = backgroundTaskService;
    }

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

    public void Disable()
    {
        _backgroundTaskService.ToggleQuickResumeStartupTask(false);
    }
}
