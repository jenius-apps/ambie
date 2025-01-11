using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AmbientSounds.ViewModels;

public partial class ActiveTrackViewModel : ObservableObject
{
    private readonly IMixMediaPlayerService _player;
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly ISoundVolumeService _soundVolumeService;

    public ActiveTrackViewModel(
        Sound s,
        IRelayCommand<Sound> removeCommand,
        IMixMediaPlayerService player,
        IAssetLocalizer assetLocalizer,
        ISoundVolumeService soundVolumeService)
    {
        _assetLocalizer = assetLocalizer;
        Sound = s;
        _player = player;
        RemoveCommand = removeCommand;
        _soundVolumeService = soundVolumeService;

        _volume = _soundVolumeService.GetVolume(Sound.Id, _player.CurrentMixId);
    }

    /// <summary>
    /// The <see cref="Sound"/>
    /// for this view model.
    /// </summary>
    public Sound Sound { get; }

    [ObservableProperty]
    private double _volume;

    /// <summary>
    /// The name of the sound.
    /// </summary>
    public string Name => Sound.IsMix ? Sound.Name : _assetLocalizer.GetLocalName(Sound);

    /// <summary>
    /// Image for the sound.
    /// </summary>
    public string ImagePath => Sound.ImagePath;

    public string ColourHex => Sound.ColourHex;

    /// <summary>
    /// This command will remove
    /// this sound from the active tracks list
    /// and it will pause it.
    /// </summary>
    public IRelayCommand<Sound> RemoveCommand { get; }

    public override string ToString()
    {
        return Name;
    }

    partial void OnVolumeChanged(double value)
    {
        _player.SetVolume(Sound.Id, value / 100d);
        _soundVolumeService.SetVolume(value, Sound.Id, _player.CurrentMixId);
    }
}
