using AmbientSounds.Events;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class ChannelViewModel : ObservableObject
{
    private readonly Channel _channel;
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly INavigator _navigator;
    private readonly IChannelService _channelService;

    public ChannelViewModel(
        Channel channel,
        IAssetLocalizer assetLocalizer,
        INavigator navigator,
        IChannelService channelService)
    {
        _channel = channel;
        _assetLocalizer = assetLocalizer;
        _navigator = navigator;
        _channelService = channelService;

        DownloadProgress = new Progress<double>();
        DownloadProgress.ProgressChanged += OnProgressChanged;
    }

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
    private double _downloadProgressValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadProgressVisible))]
    private bool _downloadLoading;

    [ObservableProperty]
    private bool _downloadProgressVisible;

    public async Task InitializeAsync()
    {
        ActionButtonLoading = true;

        var isOwnedTask = _channelService.IsOwnedAsync(_channel);
        var isFullyDownloadedTask = _channelService.IsFullyDownloadedAsync(_channel);

        IsOwned = await isOwnedTask;
        IsFullyDownloaded = await isFullyDownloadedTask;

        ActionButtonLoading = false;
    }

    [RelayCommand]
    private async Task PlayAsync()
    {
        await Task.Delay(1);
        if (_channel.Type is ChannelType.DarkScreen or ChannelType.Slideshow)
        {
            _navigator.ToScreensaver(new ScreensaverArgs { RequestedType = _channel.Type });
        }
    }

    [RelayCommand]
    private async Task DownloadAsync()
    {
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
        }

        DownloadProgressValue = e;
    }
}
