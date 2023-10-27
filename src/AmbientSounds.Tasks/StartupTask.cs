using JeniusApps.Common.Tools.Uwp;
using Windows.ApplicationModel.Background;

namespace AmbientSounds.Tasks;

public sealed class StartupTask : IBackgroundTask
{
    public void Run(IBackgroundTaskInstance taskInstance)
    {
        new ToastService().SendToast("out of proc", "hello", tag: "startupToast");
    }
}
