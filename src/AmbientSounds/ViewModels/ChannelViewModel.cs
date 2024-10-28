using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Telemetry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class ChannelViewModel : ObservableObject
{
    private readonly Channel _channel;
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly IChannelService _channelService;
    private readonly IDialogService _dialogService;
    private readonly IIapService _iapService;
    private readonly ITelemetry _telemetry;
    private bool _eventsRegistered;

    public ChannelViewModel(
        Channel channel,
        IAssetLocalizer assetLocalizer,
        IChannelService channelService,
        IDialogService dialogService,
        IIapService iapService,
        ITelemetry telemetry,
        IRelayCommand<ChannelViewModel>? viewDetailsCommand = null,
        IRelayCommand<ChannelViewModel>? changeChannelCommand = null)
    {
        _channel = channel;
        _assetLocalizer = assetLocalizer;
        _channelService = channelService;
        _dialogService = dialogService;
        _iapService = iapService;
        _telemetry = telemetry;
        ViewDetailsCommand = viewDetailsCommand ?? new RelayCommand<ChannelViewModel>(static (vm) => { });
        ChangeChannelCommand = changeChannelCommand ?? new RelayCommand<ChannelViewModel>(static (c) => { });

        DownloadProgress = new Progress<double>();
        DownloadProgress.ProgressChanged += OnProgressChanged;
    }

    public string Id => _channel.Id;

    public Channel Channel => _channel;

    public IRelayCommand<ChannelViewModel> ViewDetailsCommand { get; }

    public IRelayCommand<ChannelViewModel> ChangeChannelCommand { get; }

    public Progress<double> DownloadProgress { get; }

    public string Name => _assetLocalizer.GetLocalName(_channel);

    public string Description => _assetLocalizer.GetLocalDescription(_channel);

    public string ImagePath => _channel.ImagePath;

    public bool DownloadButtonVisible => !ActionButtonLoading && IsOwned && !IsFullyDownloaded;

    public bool PlayButtonVisible => !ActionButtonLoading && IsOwned && IsFullyDownloaded;

    public bool BuyButtonVisible => !ActionButtonLoading && !IsOwned;

    public bool ScreensaverBackplateVisible => BuyButtonVisible || DownloadButtonVisible || DownloadProgressVisible;

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
            if (ActionButtonLoading || DownloadProgressVisible || DownloadLoading)
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
        }

        var isOwnedTask = _channelService.IsOwnedAsync(_channel);
        var isFullyDownloadedTask = _channelService.IsFullyDownloadedAsync(_channel);

        IsOwned = await isOwnedTask;
        IsFullyDownloaded = await isFullyDownloadedTask;

        ActionButtonLoading = false;
    }

    public void Uninitialize()
    {
        _iapService.ProductPurchased -= OnProductPurchased;
    }

    [RelayCommand]
    private async Task Play()
    {
        await _channelService.PlayChannelAsync(_channel);
        _telemetry.TrackEvent(TelemetryConstants.ChannelPlayed, new Dictionary<string, string>
        {
            { "name", _channel.Name }
        });
    }

    [RelayCommand]
    private async Task UnlockAsync()
    {
        ActionButtonLoading = true;
        _telemetry.TrackEvent(TelemetryConstants.ChannelUnlockClicked, new Dictionary<string, string>
        {
            { "name", _channel.Name }
        });

        await _dialogService.OpenPremiumAsync();
        ActionButtonLoading = false;
    }

    [RelayCommand]
    private async Task DownloadAsync()
    {
        ActionButtonLoading = true;
        ViewDetailsCommand.Execute(this);
        await Task.Delay(600);
        await _channelService.QueueInstallChannelAsync(_channel, DownloadProgress);
        _telemetry.TrackEvent(TelemetryConstants.ChannelDownloadClicked, new Dictionary<string, string>
        {
            { "name", _channel.Name }
        });
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
        }

        DownloadProgressValue = e;
    }

    private void OnProductPurchased(object sender, string iapId)
    {
        if (_channel.IapIds.Contains(iapId))
        {
            IsOwned = true;
        }
    }
}
