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

    /// <inheritdoc/>
    public async Task<bool> RequestPermissionAsync()
    {
        var result = await BackgroundExecutionManager.RequestAccessAsync();
        return result is BackgroundAccessStatus.AlwaysAllowed
            or BackgroundAccessStatus.AllowedSubjectToSystemPolicy;
    }

    /// <inheritdoc/>
    public void TogglePushNotificationRenewalTask(bool enable)
    {
        var taskType = typeof(PushNotificationRenewalTask);

        // To avoid duplicate registrations, always unregister no matter the "enable" value.
        UnregisterTask(taskType.Name);

        if (enable)
        {
            var builder = new BackgroundTaskBuilder
            {
                Name = taskType.Name,
                TaskEntryPoint = taskType.FullName
            };
            builder.SetTrigger(new TimeTrigger(20160, false)); // 14 days = 20160 minutes 
            builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            builder.Register();
        }
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void ToggleStreakReminderTask(bool enable)
    {
        var taskType = typeof(StreakReminderTask);

        if (!enable)
        {
            UnregisterTask(taskType.Name);
            return;
        }

        var builder = new BackgroundTaskBuilder
        {
            Name = taskType.Name,
            TaskEntryPoint = taskType.FullName
        };
        builder.SetTrigger(new TimeTrigger(60, false));
        builder.AddCondition(new SystemCondition(SystemConditionType.UserPresent));
        builder.Register();
    }

    /// <inheritdoc/>
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
