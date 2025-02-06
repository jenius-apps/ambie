using AmbientSounds.Constants;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Tools;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

/// <summary>
/// View model for a player user control.
/// </summary>
public partial class PlayerViewModel : ObservableObject
{
    private readonly IMixMediaPlayerService _player;
    private readonly IUserSettings _userSettings;
    private readonly IDispatcherQueue _dispatcherQueue;

    public PlayerViewModel(
        IMixMediaPlayerService player,
        IUserSettings userSettings,
        IDispatcherQueue dispatcherQueue)
    {
        Guard.IsNotNull(player);
        Guard.IsNotNull(userSettings);
        Guard.IsNotNull(dispatcherQueue);

        _player = player;
        _userSettings = userSettings;
        _dispatcherQueue = dispatcherQueue;

        Volume = userSettings.Get<double>(UserSettingsConstants.Volume);
    }

    /// <summary>
    /// Flag for if the player is playing or is about to.
    /// </summary>
    public bool IsPlaying => _player.PlaybackState == MediaPlaybackState.Playing || _player.PlaybackState == MediaPlaybackState.Opening;

    /// <summary>
    /// For for if player is paused or otherwise not playing.
    /// </summary>
    public bool IsPaused => !IsPlaying;

    /// <summary>
    /// Volume of player. Range of 0 to 100.
    /// </summary>
    public double Volume
    {
        get => _player.GlobalVolume * 100;
        set
        {
            _player.GlobalVolume = value / 100d;
            _userSettings.Set(UserSettingsConstants.Volume, value);
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Toggles the player's state.
    /// </summary>
    [RelayCommand]
    private async Task TogglePlayStateAsync()
    {
        if (IsPlaying) _player.Pause();
        else _player.Play();

        await Task.Delay(100);

        UpdatePlayState();
    }

    private void UpdatePlayState()
    {
        OnPropertyChanged(nameof(IsPlaying));
        OnPropertyChanged(nameof(IsPaused));
    }

    private void PlaybackStateChanged(object sender, MediaPlaybackState state)
    {
        _dispatcherQueue.TryEnqueue(UpdatePlayState);
    }

    public void Initialize()
    {
        _player.PlaybackStateChanged += PlaybackStateChanged;

        // Required to update the binding
        // when returning to main page (because main is cached).
        OnPropertyChanged(nameof(Volume));
    }

    public void Dispose()
    {
        _player.PlaybackStateChanged -= PlaybackStateChanged;
    }
}
