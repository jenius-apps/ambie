using AmbientSounds.Factories;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class UpdatesViewModel : ObservableObject
{
    private readonly IUpdateService _updateService;
    private readonly ISoundVmFactory _soundVmFactory;
    private CancellationTokenSource _cts;

    public UpdatesViewModel(
        IUpdateService updateService,
        ISoundVmFactory soundVmFactory,
        IOnlineSoundDataProvider onlineSoundDataProvider)
    {
        Guard.IsNotNull(updateService);
        Guard.IsNotNull(soundVmFactory);
        Guard.IsNotNull(onlineSoundDataProvider);

        _updateService = updateService;
        _soundVmFactory = soundVmFactory;
        _cts = new();
    }

    public ObservableCollection<OnlineSoundViewModel> UpdateList { get; } = new();

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
            foreach (var s in availableUpdates)
            {
                var vm = _soundVmFactory.GetOnlineSoundVm(s);
                vm.UpdateAvailable = true;
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
            vm.Dispose();
        }

        UpdateList.Clear();
    }
}
