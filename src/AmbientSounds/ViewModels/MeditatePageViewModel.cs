using AmbientSounds.Cache;
using AmbientSounds.Factories;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class MeditatePageViewModel : ObservableObject
{
    private readonly ISoundCache _soundCache;
    private readonly ISoundVmFactory _soundVmFactory;

    public MeditatePageViewModel(
        ISoundCache soundCache,
        ISoundVmFactory soundVmFactory)
    {
        _soundCache = soundCache;
        _soundVmFactory = soundVmFactory;
    }

    public ObservableCollection<OnlineSoundViewModel> Guides { get; } = new();

    public async Task InitializeAsync()
    {
        var sounds = await _soundCache.GetOnlineSoundsAsync();
        foreach (var s in sounds)
        {
            var vm = _soundVmFactory.GetOnlineSoundVm(s);
            // TODO initialize vm
            Guides.Add(vm);
        }
    }

    public void Uninitialize()
    {
        Guides.Clear();
    }
}
