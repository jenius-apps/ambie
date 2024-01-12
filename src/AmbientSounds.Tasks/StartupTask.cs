using AmbientSounds.Constants;
using JeniusApps.Common.Tools.Uwp;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;

#nullable enable

namespace AmbientSounds.Tasks;

public sealed class StartupTask : IBackgroundTask
{
    public void Run(IBackgroundTaskInstance taskInstance)
    {
        var resourceLoader = ResourceLoader.GetForViewIndependentUse();
        new ToastService().SendToast(
            resourceLoader.GetString("QuickResumeTitle"),
            resourceLoader.GetString("QuickResumeToastMessage"),
            launchArg: LaunchConstants.QuickResumeArgument);
    }
}
