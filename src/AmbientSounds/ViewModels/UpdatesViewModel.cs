using AmbientSounds.Factories;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class UpdatesViewModel : ObservableObject
{
    private readonly ISoundService _soundService;
    private readonly ISoundVmFactory _soundVmFactory;
    private readonly IOnlineSoundDataProvider _onlineSoundDataProvider;

    public UpdatesViewModel(
        ISoundService soundService,
        ISoundVmFactory soundVmFactory,
        IOnlineSoundDataProvider onlineSoundDataProvider)
    {
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(soundVmFactory);
        Guard.IsNotNull(onlineSoundDataProvider);

        _soundService = soundService;
        _soundVmFactory = soundVmFactory;
        _onlineSoundDataProvider = onlineSoundDataProvider;
    }

    public ObservableCollection<UpdateSoundViewModel> UpdateList { get; } = new();

    [ObservableProperty]
    private bool _updateAllVisible;

    [RelayCommand]
    private async Task CheckUpdatesAsync()
    {
        UpdateAllVisible = false;
        UpdateList.Clear();
        var installed = await _soundService.GetLocalSoundsAsync();
        foreach (var s in installed)
        {
            var vm = _soundVmFactory.GetUpdateViewModel(s);
            UpdateList.Add(vm);
        }
        UpdateAllVisible = UpdateList.Count > 0;
    }

    public void Uninitialize()
    {
        UpdateList.Clear();
        UpdateAllVisible = UpdateList.Count > 0;
    }
}
