using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public interface IBackgroundTaskService
{
    void UnregisterAllTasks();

    Task<bool> RequestPermissionAsync();

    void ToggleQuickResumeStartupTask(bool enable);

    void UnregisterTask(string name);
}
