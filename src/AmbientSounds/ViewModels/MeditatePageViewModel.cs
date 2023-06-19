using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class MeditatePageViewModel : ObservableObject
{
    private readonly IGuideService _guideService;
    private readonly IGuideVmFactory _guideVmFactory;
    private readonly IMixMediaPlayerService _mixMediaPlayerService;

    public MeditatePageViewModel(
        IGuideService guideService,
        IGuideVmFactory guideVmFactory,
        IMixMediaPlayerService mixMediaPlayerService)
    {
        _guideService = guideService;
        _guideVmFactory = guideVmFactory;
        _mixMediaPlayerService = mixMediaPlayerService;
    }

    public ObservableCollection<GuideViewModel> Guides { get; } = new();

    public async Task InitializeAsync()
    {
        _mixMediaPlayerService.PlaybackStateChanged += OnPlaybackChanged;

        if (Guides.Count > 0)
        {
            return;
        }

        var guides = await _guideService.GetOnlineGuidesAsync(culture: "en"); // TODO support other languages
        foreach (var guide in guides)
        {
            var vm = _guideVmFactory.GetOrCreate(
                guide,
                DownloadCommand,
                DeleteCommand,
                PlayGuideCommand,
                PauseGuideCommand
                /* downloadProgress: TODO */);

            Guide? offlineGuide = await _guideService.GetOfflineGuideAsync(guide.Id);
            vm.IsDownloaded = offlineGuide is not null;
            vm.IsPlaying = _mixMediaPlayerService.IsSoundPlaying(guide.Id);
            Guides.Add(vm);
        }
    }

    public void Uninitialize()
    {
        _mixMediaPlayerService.PlaybackStateChanged -= OnPlaybackChanged;
        Guides.Clear();
    }

    [RelayCommand]
    private async Task DownloadAsync(GuideViewModel? guideVm)
    {
        if (guideVm is null || guideVm.Loading || guideVm.DownloadProgressVisible)
        {
            return;
        }

        guideVm.Loading = true;

        try
        {
            await _guideService.DownloadAsync(guideVm.OnlineGuide, guideVm.DownloadProgress);
        }
        catch (TaskCanceledException)
        {
            guideVm.Loading = false;
        }
    }

    [RelayCommand]
    private async Task PlayGuideAsync(GuideViewModel? guideVm)
    {
        if (guideVm is null || !guideVm.IsDownloaded)
        {
            return;
        }

        if (_mixMediaPlayerService.IsSoundPlaying(guideVm.OnlineGuide.Id))
        {
            _mixMediaPlayerService.Play();
            return;
        }

        if (await _guideService.GetOfflineGuideAsync(guideVm.OnlineGuide.Id) is Guide offlineGuide)
        {
            // Only an offline guide can be played because its file is saved locally
            await _mixMediaPlayerService.PlayGuideAsync(offlineGuide);
        }
    }

    [RelayCommand]
    private void PauseGuide(GuideViewModel? guideVm)
    {
        if (guideVm is not null)
        {
            _mixMediaPlayerService.Pause();
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(GuideViewModel? guideVm)
    {
        if (guideVm?.OnlineGuide is { Id: string guideId })
        {
            bool deleted = await _guideService.DeleteAsync(guideId);
            guideVm.IsDownloaded = !deleted;
        }
    }

    private void OnPlaybackChanged(object sender, MediaPlaybackState updatedState)
    {
        // preserve currentGuideId in case it changes during the loop.
        string currentGuideId = _mixMediaPlayerService.CurrentGuideId;
        foreach (var guideVm in Guides)
        {
            guideVm.IsPlaying = updatedState is MediaPlaybackState.Playing
                && guideVm.OnlineGuide.Id == currentGuideId;
        }
    }
}
