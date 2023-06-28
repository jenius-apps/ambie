using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Tools;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class MeditatePageViewModel : ObservableObject
{
    private readonly IGuideService _guideService;
    private readonly IGuideVmFactory _guideVmFactory;
    private readonly IMixMediaPlayerService _mixMediaPlayerService;
    private readonly IDialogService _dialogService;
    private readonly IIapService _iapService;
    private readonly IDispatcherQueue _dispatcherQueue;

    public MeditatePageViewModel(
        IGuideService guideService,
        IGuideVmFactory guideVmFactory,
        IDialogService dialogService,
        IIapService iapService,
        IMixMediaPlayerService mixMediaPlayerService,
        IDispatcherQueue dispatcherQueue)
    {
        _guideService = guideService;
        _guideVmFactory = guideVmFactory;
        _mixMediaPlayerService = mixMediaPlayerService;
        _dialogService = dialogService;
        _iapService = iapService;
        _dispatcherQueue = dispatcherQueue;
    }

    public ObservableCollection<GuideViewModel> Guides { get; } = new();

    public async Task InitializeAsync()
    {
        _mixMediaPlayerService.PlaybackStateChanged += OnPlaybackChanged;
        _iapService.ProductPurchased += OnProductPurchased;
        _guideService.GuideStopped += OnGuideStopped;

        if (Guides.Count > 0)
        {
            return;
        }

        var guides = await _guideService.GetOnlineGuidesAsync();
        foreach (var guide in guides.OrderBy(static x => x.Id))
        {
            var vm = _guideVmFactory.Create(
                guide,
                DownloadCommand,
                DeleteCommand,
                PlayGuideCommand,
                StopGuideCommand,
                PurchaseCommand
                /* downloadProgress: TODO */);

            vm.Initialize();

            Guide? offlineGuide = await _guideService.GetOfflineGuideAsync(guide.Id);
            vm.IsDownloaded = offlineGuide is not null;
            vm.IsPlaying = _mixMediaPlayerService.CurrentGuideId == guide.Id 
                && _mixMediaPlayerService.PlaybackState is MediaPlaybackState.Playing;
            vm.IsOwned = await _iapService.IsAnyOwnedAsync(guide.IapIds);
            Guides.Add(vm);
        }
    }

    public void Uninitialize()
    {
        _mixMediaPlayerService.PlaybackStateChanged -= OnPlaybackChanged;
        _iapService.ProductPurchased -= OnProductPurchased;
        _guideService.GuideStopped -= OnGuideStopped;

        foreach (var g in Guides)
        {
            g.Uninitialize();
        }

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

        await _guideService.PlayAsync(guideVm.OnlineGuide);
    }

    [RelayCommand]
    private void StopGuide(GuideViewModel? guideVm)
    {
        if (guideVm is not null)
        {
            _guideService.Stop(guideVm.OnlineGuide.Id);
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

    [RelayCommand]
    private async Task PurchaseAsync()
    {
        await _dialogService.OpenPremiumAsync();
    }

    private void OnPlaybackChanged(object sender, MediaPlaybackState updatedState)
    {
        // This ensures that when a guide starts playing, the play icon changes
        // to the stop icon.
        string currentGuideId = _mixMediaPlayerService.CurrentGuideId;
        foreach (var guideVm in Guides)
        {
            guideVm.IsPlaying = guideVm.OnlineGuide.Id == currentGuideId;
        }
    }

    private void OnProductPurchased(object sender, string purchasedIapId)
    {
        foreach (var guideVm in Guides)
        {
            if (guideVm.OnlineGuide.IapIds.Contains(purchasedIapId))
            {
                guideVm.IsOwned = true;
            }
        }
    }

    private void OnGuideStopped(object sender, string e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            foreach (var guideVm in Guides)
            {
                guideVm.IsPlaying = false;
            }
        });
    }
}
