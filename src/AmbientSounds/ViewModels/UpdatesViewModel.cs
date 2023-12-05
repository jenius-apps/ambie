using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using JeniusApps.Common.Telemetry;

namespace AmbientSounds.ViewModels;

public partial class UpdatesViewModel : ObservableObject
{
    private readonly IUpdateService _updateService;
    private readonly ISoundVmFactory _soundVmFactory;
    private readonly ITelemetry _telemetry;
    private CancellationTokenSource _cts;

    public UpdatesViewModel(
        IUpdateService updateService,
        ISoundVmFactory soundVmFactory,
        ITelemetry telemetry)
    {
        Guard.IsNotNull(updateService);
        Guard.IsNotNull(soundVmFactory);
        Guard.IsNotNull(telemetry);

        _updateService = updateService;
        _soundVmFactory = soundVmFactory;
        _telemetry = telemetry;
        _cts = new();
    }

    public ObservableCollection<VersionedAssetViewModel> UpdateList { get; } = new();

    [ObservableProperty]
    private bool _updateAllVisible;

    [ObservableProperty]
    private bool _placeholderVisible;

    [RelayCommand]
    private async Task CheckUpdatesAsync()
    {
        UpdateAllVisible = false;
        PlaceholderVisible = false;

        try
        {
            var availableUpdatesTask = _updateService.CheckForUpdatesAsync(_cts.Token);
            ClearUpdateList();

            var availableUpdates = await availableUpdatesTask;
            foreach ((IVersionedAsset asset, UpdateReason r) in availableUpdates)
            {
                var vm = _soundVmFactory.GetVersionAssetVm(asset, r);
                await vm.InitializeAsync();
                UpdateList.Add(vm);
            }
        }
        catch (OperationCanceledException)
        {
            return;
        }

        UpdateAllVisible = UpdateList.Count > 0;
        PlaceholderVisible = UpdateList.Count == 0;
    }

    [RelayCommand]
    private async Task UpdateAllAsync()
    {
        _telemetry.TrackEvent(TelemetryConstants.UpdateAllClicked);

        List<Task> tasks = new();

        foreach (var vm in UpdateList)
        {
            tasks.Add(vm.UpdateCommand.ExecuteAsync(null));
        }

        await Task.WhenAll(tasks);
    }

    public void Uninitialize()
    {
        _cts.Cancel();
        ClearUpdateList();
        UpdateAllVisible = false;
        PlaceholderVisible = false;
        _cts.Dispose();
        _cts = new();
    }

    private void ClearUpdateList()
    {
        foreach (var vm in UpdateList)
        {
            vm.Unitialize();
        }

        UpdateList.Clear();
    }
}
