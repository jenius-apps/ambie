using AmbientSounds.Tasks;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

#nullable enable

namespace AmbientSounds.Services.Uwp;

public class BackgroundTaskService : IBackgroundTaskService
{
    public void UnregisterAllTasks()
    {
        foreach (var bgTask in BackgroundTaskRegistration.AllTasks)
        {
            bgTask.Value.Unregister(cancelTask: true);
        }
    }

    public async Task<bool> RequestPermissionAsync()
    {
        var result = await BackgroundExecutionManager.RequestAccessAsync();
        return result is BackgroundAccessStatus.AlwaysAllowed
            or BackgroundAccessStatus.AllowedSubjectToSystemPolicy;
    }

    public void ToggleQuickResumeStartupTask(bool enable)
    {
        var taskType = typeof(StartupTask);

        if (enable)
        {
            var builder = new BackgroundTaskBuilder
            {
                Name = taskType.Name,
                TaskEntryPoint = taskType.FullName
            };
            builder.SetTrigger(new SystemTrigger(SystemTriggerType.SessionConnected, false));
            builder.AddCondition(new SystemCondition(SystemConditionType.UserPresent));
            builder.Register();
        }
        else
        {
            UnregisterTask(taskType.Name);
        }
    }

    public void UnregisterTask(string name)
    {
        foreach (var bgTask in BackgroundTaskRegistration.AllTasks)
        {
            if (bgTask.Value.Name == name)
            {
                bgTask.Value.Unregister(cancelTask: true);
                return;
            }
        }
    }
}
