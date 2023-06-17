using AmbientSounds.Factories;
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
        if (Guides.Count > 0)
        {
            return;
        }

        var guides = await _guideService.GetGuidesAsync(culture: "en"); // TODO support other languages
        foreach (var guide in guides)
        {
            var vm = _guideVmFactory.GetOrCreate(
                guide,
                DownloadCommand,
                DeleteCommand,
                PlayGuideCommand,
                PauseGuideCommand
                /* downloadProgress: TODO */);
            Guides.Add(vm);
        }
    }

    public void Uninitialize()
    {
        Guides.Clear();
    }

    [RelayCommand]
    private async Task DownloadAsync(GuideViewModel? guideVm)
    {
        if (guideVm?.DownloadProgress is null || guideVm.Loading || guideVm.DownloadProgressVisible)
        {
            return;
        }

        guideVm.Loading = true;

        try
        {
            await _guideService.DownloadAsync(guideVm.Guide, guideVm.DownloadProgress);
        }
        catch (TaskCanceledException)
        {
            guideVm.Loading = false;
        }
    }

    [RelayCommand]
    private async Task PlayGuideAsync(GuideViewModel? guideVm)
    {
        if (guideVm is null ||
            !guideVm.IsDownloaded ||
            _mixMediaPlayerService.IsSoundPlaying(guideVm.Guide.Id))
        {
            return;
        }

        if (_guideService.GetCachedGuide(guideVm.Guide.Id) is { } guide)
        {
            // Retrieve latest cached guide to ensure we have the
            // offline version. This fixes the bug where a guide VM
            // will still hold the online version even when it was just downloaded.
            await _mixMediaPlayerService.PlayGuideAsync(guide);
        }
    }

    [RelayCommand]
    private void PauseGuide(GuideViewModel? guideVm)
    {
        _mixMediaPlayerService.Pause();
    }

    [RelayCommand]
    private async Task DeleteAsync(GuideViewModel? guideVm)
    {
        if (guideVm?.Guide is { Id: string guideId })
        {
            bool deleted = await _guideService.DeleteAsync(guideId);
            guideVm.IsDownloaded = !deleted;
        }
    }
}
