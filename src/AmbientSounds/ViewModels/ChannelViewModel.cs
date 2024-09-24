using AmbientSounds.Events;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    }

    public string Name => _assetLocalizer.GetLocalName(_channel);

    public string Description => _assetLocalizer.GetLocalDescription(_channel);

    public string ImagePath => _channel.ImagePath;

    [ObservableProperty]
    private bool _downloadButtonVisible;

    [ObservableProperty]
    private bool _playButtonVisible;

    [ObservableProperty]
    private bool _actionButtonLoading;

    public async Task InitializeAsync()
    {
        ActionButtonLoading = true;

        await Task.Delay(1000);

        var isFullyDownloaded = await _channelService.IsFullyDownloadedAsync(_channel);
        if (isFullyDownloaded)
        {
            PlayButtonVisible = true;
        }
        else
        {
            DownloadButtonVisible = true;
        }

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
}
