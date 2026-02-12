using AmbientSounds.Cache;
using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Store;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
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
    private readonly ITelemetry _telemetry;
    private readonly IPageCache _pageCache;
    private readonly ICatalogueRowVmFactory _catalogueRowVmFactory;
    private readonly ISoundService _soundService;
    private readonly ISoundVmFactory _soundVmFactory;

    public MeditatePageViewModel(
        IGuideService guideService,
        IGuideVmFactory guideVmFactory,
        IDialogService dialogService,
        IIapService iapService,
        IMixMediaPlayerService mixMediaPlayerService,
        IDispatcherQueue dispatcherQueue,
        ITelemetry telemetry,
        IPageCache pageCache,
        ICatalogueRowVmFactory catalogueRowVmFactory,
        ISoundService soundService,
        ISoundVmFactory soundVmFactory)
    {
        _guideService = guideService;
        _guideVmFactory = guideVmFactory;
        _mixMediaPlayerService = mixMediaPlayerService;
        _dialogService = dialogService;
        _iapService = iapService;
        _dispatcherQueue = dispatcherQueue;
        _telemetry = telemetry;
        _pageCache = pageCache;
        _catalogueRowVmFactory = catalogueRowVmFactory;
        _soundService = soundService;
        _soundVmFactory = soundVmFactory;

        SavedMixes.CollectionChanged += OnSavedMixesCollectionChanged;
    }

    public ObservableCollection<SoundViewModel> SavedMixes { get; } = [];

    public ObservableCollection<GuideViewModel> Guides { get; } = [];

    public ObservableCollection<CatalogueRowViewModel> Rows { get; } = [];

    [ObservableProperty]
    private bool _placeholderVisible;

    /// <summary>
    /// Determines if the saved mixes UI is visible.
    /// </summary>
    public bool SavedMixesVisible => SavedMixes.Count > 0;

    public async Task InitializeAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        _mixMediaPlayerService.PlaybackStateChanged += OnPlaybackChanged;
        _iapService.ProductPurchased += OnProductPurchased;
        _guideService.GuideStopped += OnGuideStopped;
        _soundService.LocalSoundAdded += OnLocalSoundAdded;
        _soundService.LocalSoundDeleted += OnLocalSoundDeleted;

        if (Guides.Count > 0)
        {
            return;
        }

        PlaceholderVisible = true;
        Task<IReadOnlyList<Guide>> guidesTask = _guideService.GetOnlineGuidesAsync();

        // Include delay to account for shimmer effect.
        await Task.WhenAll(guidesTask, Task.Delay(300, ct));
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<Guide> guides = await guidesTask;

        foreach (Guide? guide in guides.OrderBy(static x => x.Id))
        {
            ct.ThrowIfCancellationRequested();
            GuideViewModel vm = _guideVmFactory.Create(
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
            vm.IsPlaying = _mixMediaPlayerService.FeaturedSoundId == guide.Id
                && _mixMediaPlayerService.PlaybackState is MediaPlaybackState.Playing;
            vm.IsOwned = !guide.IsPremium || await _iapService.IsAnyOwnedAsync(guide.IapIds);
            Guides.Add(vm);

            if (PlaceholderVisible)
            {
                PlaceholderVisible = false;
            }
        }

        await LoadMixesAsync(ct);
        await LoadRowsAsync(ct);
    }

    private async Task LoadMixesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<Sound> mixes = await _soundService.GetLocalMixesAsync(tag: AssetTagConstants.MeditatePageTag);
        ct.ThrowIfCancellationRequested();
        foreach (Sound mix in mixes.OrderBy(x => x.Name))
        {
            ct.ThrowIfCancellationRequested();
            SoundViewModel vm = _soundVmFactory.GetSoundVm(mix);
            SavedMixes.Add(vm);
        }
    }

    private async Task LoadRowsAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<CatalogueRow> rows = await _pageCache.GetMeditatePageRowsAsync(ct);
        ct.ThrowIfCancellationRequested();

        List<Task> tasks = [];
        foreach (CatalogueRow row in rows)
        {
            ct.ThrowIfCancellationRequested();
            CatalogueRowViewModel vm = _catalogueRowVmFactory.Create(row);
            tasks.Add(vm.LoadAsync(null, ct));
            Rows.Add(vm);
        }

        await Task.WhenAll(tasks);
    }

    public void Uninitialize()
    {
        _mixMediaPlayerService.PlaybackStateChanged -= OnPlaybackChanged;
        _iapService.ProductPurchased -= OnProductPurchased;
        _guideService.GuideStopped -= OnGuideStopped;
        _soundService.LocalSoundAdded -= OnLocalSoundAdded;
        _soundService.LocalSoundDeleted -= OnLocalSoundDeleted;

        foreach (GuideViewModel g in Guides)
        {
            g.Uninitialize();
        }
        Guides.Clear();

        foreach (CatalogueRowViewModel row in Rows)
        {
            row.Uninitialize();
        }
        Rows.Clear();

        foreach (SoundViewModel mixVm in SavedMixes)
        {
            mixVm.Dispose();
        }
        SavedMixes.Clear();
    }

    private void OnLocalSoundAdded(object sender, Sound newSound)
    {
        if (!newSound.IsMix)
        {
            return;
        }

        SoundViewModel vm = _soundVmFactory.GetSoundVm(newSound);
        _dispatcherQueue.TryEnqueue(() => SavedMixes.Add(vm));
    }

    private void OnLocalSoundDeleted(object sender, string removedSoundId)
    {
        SoundViewModel? mixToRemove = SavedMixes.FirstOrDefault(x => x.Id == removedSoundId);
        if (mixToRemove is null)
        {
            return;
        }

        _dispatcherQueue.TryEnqueue(() => SavedMixes.Remove(mixToRemove));
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
            _telemetry.TrackEvent(TelemetryConstants.GuideDownloaded, new Dictionary<string, string>
            {
                { "name", guideVm.Name },
                { "guideId", guideVm.OnlineGuide.Id }
            });
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
        _telemetry.TrackEvent(TelemetryConstants.GuidePlayed, new Dictionary<string, string>
        {
            { "name", guideVm.Name }
        });
    }

    [RelayCommand]
    private void StopGuide(GuideViewModel? guideVm)
    {
        if (guideVm is not null)
        {
            _guideService.Stop(guideVm.OnlineGuide.Id);
            _telemetry.TrackEvent(TelemetryConstants.GuideStopped, new Dictionary<string, string>
            {
                { "name", guideVm.Name }
            });
        }
    }

    [RelayCommand]
    private async Task DeleteAsync(GuideViewModel? guideVm)
    {
        if (guideVm?.OnlineGuide is { Id: string guideId })
        {
            bool deleted = await _guideService.DeleteAsync(guideId);
            guideVm.IsDownloaded = !deleted;

            if (deleted)
            {
                _telemetry.TrackEvent(TelemetryConstants.GuideDeleted, new Dictionary<string, string>
                {
                    { "name", guideVm.Name }
                });
            }
        }
    }

    [RelayCommand]
    private async Task PurchaseAsync()
    {
        await _dialogService.OpenPremiumAsync();
        _telemetry.TrackEvent(TelemetryConstants.GuidePurchaseClicked);
    }

    private void OnPlaybackChanged(object sender, MediaPlaybackState updatedState)
    {
        // This ensures that when a guide starts playing, the play icon changes
        // to the stop icon.
        string currentGuideId = _mixMediaPlayerService.FeaturedSoundId;
        foreach (GuideViewModel guideVm in Guides)
        {
            guideVm.IsPlaying = guideVm.OnlineGuide.Id == currentGuideId;
        }
    }

    private void OnProductPurchased(object sender, string purchasedIapId)
    {
        foreach (GuideViewModel guideVm in Guides)
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
            foreach (GuideViewModel guideVm in Guides)
            {
                guideVm.IsPlaying = false;
            }
        });
    }

    private void OnSavedMixesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(SavedMixesVisible));
    }
}
