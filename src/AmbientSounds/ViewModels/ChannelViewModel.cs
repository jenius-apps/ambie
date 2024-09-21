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

    public ChannelViewModel(
        Channel channel,
        IAssetLocalizer assetLocalizer,
        INavigator navigator)
    {
        _channel = channel;
        _assetLocalizer = assetLocalizer;
        _navigator = navigator;
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

        if (_channel.Type is ChannelType.DarkScreen or ChannelType.Slideshow)
        {
            PlayButtonVisible = true;
        }
        else
        {
            // figure out if individual components are installed
        }

        ActionButtonLoading = false;
    }

    [RelayCommand]
    private async Task PlayAsync()
    {
        await Task.Delay(1);
        if (_channel.Type is ChannelType.DarkScreen or ChannelType.Slideshow)
        {
            _navigator.ToScreensaver();
        }
    }
}
