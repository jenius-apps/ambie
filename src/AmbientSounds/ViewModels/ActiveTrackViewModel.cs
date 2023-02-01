﻿using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AmbientSounds.ViewModels;

public class ActiveTrackViewModel : ObservableObject
{
    private readonly IMixMediaPlayerService _player;
    private readonly IUserSettings _userSettings;
    private readonly IAssetLocalizer _assetLocalizer;

    public ActiveTrackViewModel(
        Sound s,
        IRelayCommand<Sound> removeCommand,
        IMixMediaPlayerService player,
        IUserSettings userSettings,
        IAssetLocalizer assetLocalizer)
    {
        Guard.IsNotNull(s);
        Guard.IsNotNull(player);
        Guard.IsNotNull(removeCommand);
        Guard.IsNotNull(userSettings);
        Guard.IsNotNull(assetLocalizer);

        _userSettings = userSettings;
        _assetLocalizer = assetLocalizer;
        Sound = s;
        _player = player;
        RemoveCommand = removeCommand;
        Volume = _userSettings.Get($"{Sound.Id}:volume", 100d);
    }

    /// <summary>
    /// The <see cref="Sound"/>
    /// for this view model.
    /// </summary>
    public Sound Sound { get; }

    /// <summary>
    /// The volume of the sound.
    /// </summary>
    public double Volume
    {
        get => _player.GetVolume(Sound.Id) * 100;
        set
        {
            _player.SetVolume(Sound.Id, value / 100d);
            _userSettings.Set($"{Sound.Id}:volume", value);
        }
    }

    /// <summary>
    /// The name of the sound.
    /// </summary>
    public string Name => Sound.IsMix ? Sound.Name : _assetLocalizer.GetLocalName(Sound);

    /// <summary>
    /// Image for the sound.
    /// </summary>
    public string ImagePath => Sound.ImagePath;

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
}
