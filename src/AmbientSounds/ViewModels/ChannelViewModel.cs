using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class ChannelViewModel : ObservableObject
{
    private readonly Channel _channel;
    private readonly IAssetLocalizer _assetLocalizer;

    public ChannelViewModel(
        Channel channel,
        IAssetLocalizer assetLocalizer)
    {
        _channel = channel;
        _assetLocalizer = assetLocalizer;
    }

    public string Name => _assetLocalizer.GetLocalName(_channel);

    public string Description => _assetLocalizer.GetLocalDescription(_channel);

    /// <summary>
    /// The path for the image to display for the current sound.
    /// </summary>
    public string ImagePath => _channel.ImagePath;

    public async Task InitializeAsync()
    {
        await Task.Delay(1);
    }
}
