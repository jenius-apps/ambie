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
        ILocalizer localizer)
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

    public ObservableCollection<Sound> MixOptions { get; } = new();

    [ObservableProperty]
    private Sound _selectedMix = GetOrCreateCurrentSelection();

    public async Task LoadGuideAsync()
    {
        await LoadCommand.ExecuteAsync(null);

        MixOptions.Clear();

        MixOptions.Add(GetOrCreateCurrentSelection());

        foreach (var idList in Guide.SuggestedBackgroundSounds)
        {
            string[] split = idList.Split(';');
            var tempMix = new Sound
            {
                IsMix = true,
                Id = Guid.NewGuid().ToString(),
                Name = split[0],
                SoundIds = split
            };

            MixOptions.Add(tempMix);
        }
    }

    [RelayCommand]
    private async Task OpenDetailsAsync()
    {
        await _dialogService.OpenGuideDetailsAsync(this);
    }

    private static Sound? _currentSelectionPlaceholder;

    private static Sound GetOrCreateCurrentSelection()
    {
        // This creates a fake mix that represents
        // the "current selection of sounds". 

        _currentSelectionPlaceholder ??= new Sound
        {
            Id = "currentSelection",
            IsMix = true,
            Name = "Current selection"
        };

        return _currentSelectionPlaceholder;
    }
}
