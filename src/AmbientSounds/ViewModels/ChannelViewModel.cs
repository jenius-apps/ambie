using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Store;
using JeniusApps.Common.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class ChannelViewModel : ObservableObject
{
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly IChannelService _channelService;
    private readonly IDialogService _dialogService;
    private readonly IIapService _iapService;
    private readonly ITelemetry _telemetry;
    private bool _eventsRegistered;
    private Progress<double>? _downloadProgress;

    public ChannelViewModel(
        Channel channel,
        IAssetLocalizer assetLocalizer,
        IChannelService channelService,
        IDialogService dialogService,
        IIapService iapService,
        ITelemetry telemetry,
        IRelayCommand<ChannelViewModel>? viewDetailsCommand = null,
        IRelayCommand<ChannelViewModel>? playCommand = null,
        bool isNew = false)
    {
        Channel = channel;
        _assetLocalizer = assetLocalizer;
        _channelService = channelService;
        _dialogService = dialogService;
        _iapService = iapService;
        _telemetry = telemetry;
        ViewDetailsCommand = viewDetailsCommand ?? new RelayCommand<ChannelViewModel>(static (vm) => { });
        PlayCommand = playCommand ?? new RelayCommand<ChannelViewModel>(static (vm) => { });
        _isNew = isNew;
    }

    public string Id => Channel.Id;

    public Channel Channel { get; }

    public IRelayCommand<ChannelViewModel> ViewDetailsCommand { get; }

    public IRelayCommand<ChannelViewModel> PlayCommand { get; }

    public string Name => $"{(IsNew ? "✨ " : "")}{_assetLocalizer.GetLocalName(Channel)}";

    public string Description => _assetLocalizer.GetLocalDescription(Channel);

    public string ImagePath => Channel.ImagePath;

    public bool DownloadButtonVisible => !ActionButtonLoading && IsOwned && !IsFullyDownloaded;

    public bool PlayButtonVisible => !ActionButtonLoading && IsOwned && IsFullyDownloaded;

    public bool BuyButtonVisible => !ActionButtonLoading && !IsOwned;

    public bool DeleteButtonVisible => Channel.Type is ChannelType.Videos && IsFullyDownloaded;

    public bool ScreensaverBackplateVisible => BuyButtonVisible || DownloadButtonVisible || DownloadProgressVisible || DownloadLoading;

    [ObservableProperty]
    private bool _isNew;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    [NotifyPropertyChangedFor(nameof(PlayButtonVisible))]
    [NotifyPropertyChangedFor(nameof(BuyButtonVisible))]
    [NotifyPropertyChangedFor(nameof(ScreensaverBackplateVisible))]
    [NotifyPropertyChangedFor(nameof(PrimaryActionGlyph))]
    [NotifyPropertyChangedFor(nameof(PrimaryCommand))]
    private bool _actionButtonLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    [NotifyPropertyChangedFor(nameof(PlayButtonVisible))]
    [NotifyPropertyChangedFor(nameof(BuyButtonVisible))]
    [NotifyPropertyChangedFor(nameof(ScreensaverBackplateVisible))]
    [NotifyPropertyChangedFor(nameof(PrimaryActionGlyph))]
    [NotifyPropertyChangedFor(nameof(PrimaryCommand))]
    private bool _isOwned;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    [NotifyPropertyChangedFor(nameof(ScreensaverBackplateVisible))]
    [NotifyPropertyChangedFor(nameof(PlayButtonVisible))]
    [NotifyPropertyChangedFor(nameof(PrimaryActionGlyph))]
    [NotifyPropertyChangedFor(nameof(PrimaryCommand))]
    [NotifyPropertyChangedFor(nameof(DeleteButtonVisible))]
    private bool _isFullyDownloaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadProgressString))]
    private double _downloadProgressValue;

    public string DownloadProgressString => $"{DownloadProgressValue:N0}%";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadProgressVisible))]
    [NotifyPropertyChangedFor(nameof(ScreensaverBackplateVisible))]
    private bool _downloadLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ScreensaverBackplateVisible))]
    private bool _downloadProgressVisible;

    public string PrimaryActionGlyph
    {
        get
        {
            if (DownloadLoading || DownloadProgressVisible)
            {
                return "";
            }
            else if (ActionButtonLoading)
            {
                return "\uE946";
            }
            else if (!IsOwned)
            {
                return "\uE785";
            }
            else if (!IsFullyDownloaded)
            {
                return "\uEBD3";
            }
            else
            {
                return "\uF5B0";
            }
        }
    }

    public IRelayCommand PrimaryCommand
    {
        get
        {
            if (ActionButtonLoading || DownloadProgressVisible || DownloadLoading)
            {
                return ViewDetailsCommand;
            }
            else if (!IsOwned)
            {
                return UnlockCommand;
            }
            else if (!IsFullyDownloaded)
            {
                return DownloadCommand;
            }
            else
            {
                return PlayCommand;
            }
        }
    }

    public async Task InitializeAsync()
    {
        ActionButtonLoading = true;

        if (!_eventsRegistered)
        {
            _eventsRegistered = true;
            _iapService.ProductPurchased += OnProductPurchased;
            _downloadProgress = _channelService.TryGetActiveProgress(Channel);
            if (_downloadProgress is not null)
            {
                _downloadProgress.ProgressChanged += OnProgressChanged;
            }
        }

        var isOwnedTask = _channelService.IsOwnedAsync(Channel);
        var isFullyDownloadedTask = _channelService.IsFullyDownloadedAsync(Channel);

        IsOwned = await isOwnedTask;
        IsFullyDownloaded = await isFullyDownloadedTask;

        if (_downloadProgress is null)
        {
            ActionButtonLoading = false;
        }
    }

    public void Uninitialize()
    {
        _iapService.ProductPurchased -= OnProductPurchased;

        if (_downloadProgress is not null)
        {
            _downloadProgress.ProgressChanged -= OnProgressChanged;
        }
    }

    [RelayCommand]
    private async Task UnlockAsync()
    {
        ActionButtonLoading = true;
        _telemetry.TrackEvent(TelemetryConstants.ChannelUnlockClicked, new Dictionary<string, string>
        {
            { "name", Channel.Name }
        });

        await _dialogService.OpenPremiumAsync();
        ActionButtonLoading = false;
    }

    [RelayCommand]
    private async Task DownloadAsync()
    {
        if (_downloadProgress is not null)
        {
            return;
        }

        _downloadProgress = new Progress<double>();
        _downloadProgress.ProgressChanged += OnProgressChanged;

        ActionButtonLoading = true;
        DownloadLoading = true;
        ViewDetailsCommand.Execute(this);
        await Task.Delay(600);
        await _channelService.QueueInstallChannelAsync(Channel, _downloadProgress);
        _telemetry.TrackEvent(TelemetryConstants.ChannelDownloadClicked, new Dictionary<string, string>
        {
            { "name", Channel.Name },
            { "channelId", Channel.Id },
            { "isPremium", Channel.IsPremium.ToString() }
        });
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        ActionButtonLoading = true;
        await _channelService.DeleteChannelAsync(Channel);
        IsFullyDownloaded = await _channelService.IsFullyDownloadedAsync(Channel);
        await Task.Delay(300); // Delay to improve UX
        ActionButtonLoading = false;
    }

    private void OnProgressChanged(object sender, double e)
    {
        if (e <= 0)
        {
            DownloadLoading = true;
            DownloadProgressVisible = false;
        }
        else if (e >= 1 && e < 100)
        {
            DownloadLoading = false;
            DownloadProgressVisible = true;
        }
        else if (e >= 100)
        {
            IsFullyDownloaded = true;
            DownloadProgressVisible = false;
            ActionButtonLoading = false;

            if (_downloadProgress is { } progress)
            {
                progress.ProgressChanged -= OnProgressChanged;
                _downloadProgress = null;
            }
        }

        DownloadProgressValue = e;
    }

    private void OnProductPurchased(object sender, string iapId)
    {
        if (Channel.IapIds.Contains(iapId))
        {
            IsOwned = true;
        }
    }
}
