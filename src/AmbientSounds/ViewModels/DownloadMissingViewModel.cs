using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class DownloadMissingViewModel : ObservableObject
{
    private readonly IShareService _shareService;
    private readonly IOnlineSoundDataProvider _soundService;
    private readonly ISoundVmFactory _vmFactory;

    [ObservableProperty]
    private bool _loading;

    public DownloadMissingViewModel(
        IShareService shareService,
        IOnlineSoundDataProvider soundService,
        ISoundVmFactory vmFactory)
    {
        Guard.IsNotNull(shareService);
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(vmFactory);

        _shareService = shareService;
        _soundService = soundService;
        _vmFactory = vmFactory;
    }

    public ObservableCollection<OnlineSoundViewModel> Sounds { get; } = new();

    public async Task InitializeAsync()
    {
        if (Loading)
        {
            return;
        }
        Loading = true;

        IReadOnlyList<string>? soundIdsToInvestigate = _shareService.FailedSoundIds;
        if (soundIdsToInvestigate is { Count: > 0 })
        {
            IReadOnlyList<Sound> onlineSounds = await _soundService.GetOnlineSoundsAsync(soundIdsToInvestigate);
            foreach (var s in onlineSounds)
            {
                if (_vmFactory.GetOnlineSoundVm(s) is { } vm)
                {
                    _ = vm.LoadCommand.ExecuteAsync(null);
                    Sounds.Add(vm);
                }
            }
        }
        Loading = false;
    }
}
