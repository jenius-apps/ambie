using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class DownloadMissingViewModel : ObservableObject
{
    private const string TelemetryLocation = "shareSoundsMissingList";
    private readonly IShareService _shareService;
    private readonly IOnlineSoundDataProvider _soundService;
    private readonly ISoundVmFactory _vmFactory;
    private readonly ITelemetry _telemetry;

    [ObservableProperty]
    private bool _loading;

    public DownloadMissingViewModel(
        IShareService shareService,
        IOnlineSoundDataProvider soundService,
        ISoundVmFactory vmFactory,
        ITelemetry telemetry)
    {
        Guard.IsNotNull(shareService);
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(vmFactory);
        Guard.IsNotNull(telemetry);

        _shareService = shareService;
        _soundService = soundService;
        _vmFactory = vmFactory;
        _telemetry = telemetry;
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

    public void Uninitialize()
    {
        foreach (var s in Sounds)
        {
            s.Dispose();
        }

        Sounds.Clear();
    }
}
