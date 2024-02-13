using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Tools;

public interface IAppStoreUpdater
{
    public Task<bool> CheckForUpdatesAsync();

    public Task<bool?> TryApplyUpdatesAsync();
    Task<bool> TrySilentDownloadAndInstallAsync();
    Task<bool> TrySilentDownloadAsync();
}
