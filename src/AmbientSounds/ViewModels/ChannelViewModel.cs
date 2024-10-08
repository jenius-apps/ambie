using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
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
    private bool _eventsRegistered;

    public ChannelViewModel(
        Channel channel,
        IAssetLocalizer assetLocalizer,
        IChannelService channelService,
        IDialogService dialogService,
        IIapService iapService,
        IRelayCommand<ChannelViewModel>? viewDetailsCommand = null,
        IRelayCommand<Channel>? changeChannelCommand = null)
    {
        _channel = channel;
        _assetLocalizer = assetLocalizer;
        _channelService = channelService;
        _dialogService = dialogService;
        _iapService = iapService;
        ViewDetailsCommand = viewDetailsCommand ?? new RelayCommand<ChannelViewModel>(static (vm) => { });
        ChangeChannelCommand = changeChannelCommand ?? new RelayCommand<Channel>(static (c) => { });

        DownloadProgress = new Progress<double>();
        DownloadProgress.ProgressChanged += OnProgressChanged;
    }

    public string Id => _channel.Id;

    public Channel Channel => _channel;

    public IRelayCommand<ChannelViewModel> ViewDetailsCommand { get; }

    public IRelayCommand<Channel> ChangeChannelCommand { get; }

    public Progress<double> DownloadProgress { get; }

    public string Name => _assetLocalizer.GetLocalName(_channel);

    public string Description => _assetLocalizer.GetLocalDescription(_channel);

    public string ImagePath => _channel.ImagePath;

    public bool DownloadButtonVisible => !ActionButtonLoading && IsOwned && !IsFullyDownloaded;

    public bool PlayButtonVisible => !ActionButtonLoading && IsOwned && IsFullyDownloaded;

    public bool BuyButtonVisible => !ActionButtonLoading && !IsOwned;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    [NotifyPropertyChangedFor(nameof(PlayButtonVisible))]
    [NotifyPropertyChangedFor(nameof(BuyButtonVisible))]
    private bool _actionButtonLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    [NotifyPropertyChangedFor(nameof(PlayButtonVisible))]
    [NotifyPropertyChangedFor(nameof(BuyButtonVisible))]
    private bool _isOwned;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    [NotifyPropertyChangedFor(nameof(PlayButtonVisible))]
    private bool _isFullyDownloaded;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadProgressString))]
    private double _downloadProgressValue;

    public string DownloadProgressString => $"{DownloadProgressValue:N0}%";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadProgressVisible))]
    private bool _downloadLoading;

    [ObservableProperty]
    private bool _downloadProgressVisible;

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
    }

    [RelayCommand]
    private async Task UnlockAsync()
    {
        ActionButtonLoading = true;
        await _dialogService.OpenPremiumAsync();
        ActionButtonLoading = false;
    }

    [RelayCommand]
    private async Task DownloadAsync()
    {
        ActionButtonLoading = true;
        await _channelService.QueueInstallChannelAsync(_channel, DownloadProgress);
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
