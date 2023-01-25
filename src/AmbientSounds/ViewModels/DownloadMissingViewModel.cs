using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class DownloadMissingViewModel : ObservableObject
{
    private const string TelemetryLocation = "shareSoundsMissingList";
    private readonly IShareService _shareService;
    private readonly IOnlineSoundDataProvider _onlineSoundDataProvider;
    private readonly ISoundVmFactory _vmFactory;
    private readonly ITelemetry _telemetry;
    private readonly ISoundService _soundService;
    private readonly IMixMediaPlayerService _player;

    [ObservableProperty]
    private bool _loading;

    public DownloadMissingViewModel(
        IShareService shareService,
        IOnlineSoundDataProvider onlineSoundDataProvider,
        ISoundVmFactory vmFactory,
        ITelemetry telemetry,
        ISoundService soundService,
        IMixMediaPlayerService player)
    {
        Guard.IsNotNull(shareService);
        Guard.IsNotNull(onlineSoundDataProvider);
        Guard.IsNotNull(vmFactory);
        Guard.IsNotNull(telemetry);
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(player);

        _shareService = shareService;
        _onlineSoundDataProvider = onlineSoundDataProvider;
        _vmFactory = vmFactory;
        _telemetry = telemetry;
        _soundService = soundService;
        _player = player;
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
            IReadOnlyList<Sound> onlineSounds = await _onlineSoundDataProvider.GetOnlineSoundsAsync(soundIdsToInvestigate);
            foreach (var s in onlineSounds)
            {
                if (_vmFactory.GetOnlineSoundVm(s) is { } vm)
                {
                    vm.DownloadCompleted += OnDownloadCompleted;
                    vm.TelemetryLocation = TelemetryLocation;
                    _ = vm.LoadCommand.ExecuteAsync(null);
                    Sounds.Add(vm);
                }
            }

            _telemetry.TrackEvent(TelemetryConstants.ShareSoundsMissingLoaded, new Dictionary<string, string>
            {
                { "not installed count", Sounds.Count(x => !x.IsInstalled).ToString() }
            });
        }

        Loading = false;
    }

    private async void OnDownloadCompleted(object sender, EventArgs e)
    {
        if (sender is OnlineSoundViewModel vm)
        {
            var sound = await _soundService.GetLocalSoundAsync(vm.Id);
            if (sound is not null)
            {
                await _player.ToggleSoundAsync(sound);
            }
        }
    }

    public void Uninitialize()
    {
        foreach (var s in Sounds)
        {
            s.DownloadCompleted -= OnDownloadCompleted;
            s.Dispose();
        }

        Sounds.Clear();
    }
}
