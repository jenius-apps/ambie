using JeniusApps.Common.Tools.Uwp;
using System;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;

namespace AmbientSounds.Tasks;

public sealed class StartupTask : IBackgroundTask
{
    public void Run(IBackgroundTaskInstance taskInstance)
    {
        var resourceLoader = ResourceLoader.GetForViewIndependentUse();
        new ToastService().SendToast(
            resourceLoader.GetString("QuickResumeTitle"),
            resourceLoader.GetString("QuickResumeToastMessage"),
            launchArg: "quickResume");
    }
}
