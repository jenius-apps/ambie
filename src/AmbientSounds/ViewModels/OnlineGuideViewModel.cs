using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Humanizer.Localisation;
using JeniusApps.Common.Tools;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class OnlineGuideViewModel : OnlineSoundViewModel
{
    private readonly IGuideService _guideService;
    private readonly IMeditateService _meditateService;

    public OnlineGuideViewModel(
        Guide g,
        IDownloadManager downloadManager,
        ISoundService soundService,
        ITelemetry telemetry,
        IPreviewService previewService,
        IIapService iapService,
        IDialogService dialogService,
        IAssetLocalizer assetLocalizer,
        IMixMediaPlayerService mixMediaPlayerService,
        IUpdateService updateService,
        ILocalizer localizer,
        IGuideService guideService,
        IMeditateService meditateService)
        : base(g,
            downloadManager,
            soundService,
            telemetry,
            previewService,
            iapService,
            dialogService,
            assetLocalizer,
            mixMediaPlayerService,
            updateService,
            localizer)
    {
        _guideService = guideService;
        _meditateService = meditateService;

        SelectedMix = GetOrCreateCurrentSelection();
        Minutes = TimeSpan.FromMinutes(g.MinutesLength).Humanize(maxUnit: TimeUnit.Minute);
        PreviewText = string.Join(" • ", new string[]
        {
            Minutes,
            Description
        });
    }

    public string Minutes { get; }

    public string PreviewText { get; }

    protected Guide Guide => (Guide)base._sound;

    public ObservableCollection<SuggestedSoundViewModel> MixOptions { get; } = new();

    [ObservableProperty]
    private SuggestedSoundViewModel? _selectedMix;

    public async Task LoadGuideAsync()
    {
        await LoadCommand.ExecuteAsync(null);
        MixOptions.Clear();

        // Add the default current selection item.
        MixOptions.Add(GetOrCreateCurrentSelection());

        // Populate the rest of the suggested sounds
        var suggestedSoundMixes = await _guideService.GetSuggestedSoundMixesAsync(Guide);
        foreach (var soundMix in suggestedSoundMixes)
        {
            MixOptions.Add(new SuggestedSoundViewModel(soundMix)
            {
                Header = _localizer.GetString("SuggestedText")
            });
        }
    }

    [RelayCommand]
    private async Task OpenDetailsAsync()
    {
        await _dialogService.OpenGuideDetailsAsync(this);
    }

    [RelayCommand]
    private async Task PlayGuideAsync()
    {
        var installedVersion = await CheckIfPlayableAndGetLocalSoundAsync<Guide>();

        if (installedVersion is Guide g)
        {
            await _meditateService.PlayAsync(g);
        }
    }


    private static SuggestedSoundViewModel? _currentSelectionPlaceholder;

    private SuggestedSoundViewModel GetOrCreateCurrentSelection()
    {
        // This creates a fake mix that represents
        // the "current selection of sounds". 

        if (_currentSelectionPlaceholder is not null)
        {
            return _currentSelectionPlaceholder;
        }
        
        var sound = new Sound
        {
            Id = "currentSelectionId",
            IsMix = true,
            Name = _localizer.GetString("CurrentSelectionText")
        };

        _currentSelectionPlaceholder = new SuggestedSoundViewModel(sound);
        return _currentSelectionPlaceholder;
    }
}

public class SuggestedSoundViewModel : ObservableObject
{
    public SuggestedSoundViewModel(Sound sound)
    {
        Sound = sound;
    }

    public Sound Sound { get; }

    public string? Header { get; init; }
}
