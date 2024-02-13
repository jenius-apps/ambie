using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Services.Store;

#nullable enable

namespace AmbientSounds.Tools.Uwp;

public sealed class MicrosoftStoreUpdater : IAppStoreUpdater
{
    private StoreContext? _context;
    private IReadOnlyList<StorePackageUpdate> _updates = [];

    public event EventHandler<double>? ProgressChanged;

    public event EventHandler? UpdateAvailable;

    public async Task<bool> CheckForUpdatesAsync()
    {
        _context ??= StoreContext.GetDefault();

        // Note: This call will crash if the app is not associated with the store.
        _updates = await _context.GetAppAndOptionalStorePackageUpdatesAsync();

        if (_updates.Count > 0)
        {
            UpdateAvailable?.Invoke(this, EventArgs.Empty);
        }

        return _updates.Count > 0;
    }

    public async Task<bool?> TryApplyUpdatesAsync()    
    {
        if (_updates.Count == 0)
        {
            return null;
        }

        _context ??= StoreContext.GetDefault();

        IAsyncOperationWithProgress<StorePackageUpdateResult, StorePackageUpdateStatus> downloadOperation =
                _context.RequestDownloadAndInstallStorePackageUpdatesAsync(_updates);

        downloadOperation.Progress = (asyncInfo, progress) =>
        {
            ProgressChanged?.Invoke(null, progress.PackageDownloadProgress);
        };

        var result = await downloadOperation.AsTask();

        return result.OverallState is StorePackageUpdateState.Completed;
    }
}
