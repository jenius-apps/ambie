using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AmbientSounds.Constants;
using AmbientSounds.Services;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using Windows.Devices.Enumeration;
using Windows.Media.Core;
using Windows.Media.Devices;
using Windows.Media.Playback;
using Windows.Storage;

#nullable enable

namespace AmbientSounds.Tools.Uwp;

/// <summary>
/// Wrapper around <see cref="MediaPlayer"/>.
/// </summary>
public class OutputControlledMediaPlayer : IMediaPlayer
{
    private readonly MediaPlayer _player;
    private readonly IUserSettings _userSettings;
    private readonly ITelemetry _telemetry;
    private string _lastUsedOutputDeviceId = string.Empty;
    private TaskCompletionSource<bool>? _mediaOpenCompletionSource = null;
    private readonly SemaphoreSlim _outputDeviceLock = new(1, 1);

    public event EventHandler<TimeSpan> PositionChanged;

    public OutputControlledMediaPlayer(
        IUserSettings userSettings,
        ITelemetry telemetry,
        bool disableSystemControls = false)
    {
        _telemetry = telemetry;

        var player = new MediaPlayer();
        if (disableSystemControls)
        {
            player.CommandManager.IsEnabled = false;
        }
        _player = player;
        _player.MediaOpened += OnMediaOpened;
        _player.MediaFailed += OnMediaFailed;

        _userSettings = userSettings;
        _userSettings.SettingSet += OnUserSettingsSet;
    }

    /// <inheritdoc/>
    public double Volume
    {
        get => _player.Volume;
        set => _player.Volume = value;
    }
    public TimeSpan Duration { get; }

    /// <inheritdoc/>
    public void Play()
    {
        SetOutputDevice();
        _player.Play();
    }

    /// <inheritdoc/>
    public void Pause() => _player.Pause();

    /// <inheritdoc/>
    public void Dispose()
    {
        _userSettings.SettingSet -= OnUserSettingsSet;
        MediaDevice.DefaultAudioRenderDeviceChanged -= OnAudioDeviceChanged;
        _player.MediaOpened -= OnMediaOpened;
        _player.MediaFailed -= OnMediaFailed;
        _player.Dispose();
    }

    /// <inheritdoc/>
    public bool SetUriSource(Uri uriSource, bool enableGaplessLoop = false)
    {
        try
        {
            var mediaSource = MediaSource.CreateFromUri(uriSource);
            AssignSource(mediaSource, enableGaplessLoop);
        }
        catch
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public async Task<bool> SetSourceAsync(string pathToFile, bool enableGaplessLoop = false)
    {
        try
        {
            StorageFile file = await StorageFile.GetFileFromPathAsync(pathToFile);
            var mediaSource = MediaSource.CreateFromStorageFile(file);
            AssignSource(mediaSource, enableGaplessLoop);
        }
        catch
        {
            return false;
        }

        return true;
    }

    private void AssignSource(MediaSource source, bool enableGaplessLoop)
    {
        _player.Source = enableGaplessLoop
            ? LoopEnabledPlaybackList(source)
            : source;
    }

    private MediaPlaybackList LoopEnabledPlaybackList(MediaSource source)
    {
        // This code here (combined with a wav source file) allows for gapless playback!
        var item = new MediaPlaybackItem(source);
        var playbackList = new MediaPlaybackList() { AutoRepeatEnabled = true };
        playbackList.Items.Add(item);
        return playbackList;
    }

    private async void SetOutputDevice()
    {
        await _outputDeviceLock.WaitAsync();

        var outputDeviceId = _userSettings.Get<string>(UserSettingsConstants.OutputAudioDeviceId);
        if (outputDeviceId == _lastUsedOutputDeviceId)
        {
            _outputDeviceLock.Release();
            return;
        }

        // Fallback to use the currently selected default audio device if we can't find a valid device
        // Id from the settings. And also subscribed the event to listen to the change of the default
        // audio device.
        if (string.IsNullOrEmpty(outputDeviceId))
        {
            outputDeviceId = MediaDevice.GetDefaultAudioRenderId(AudioDeviceRole.Default);
            MediaDevice.DefaultAudioRenderDeviceChanged -= OnAudioDeviceChanged;
            MediaDevice.DefaultAudioRenderDeviceChanged += OnAudioDeviceChanged;
        }
        else
        {
            MediaDevice.DefaultAudioRenderDeviceChanged -= OnAudioDeviceChanged;
        }

        var deviceInformation = await DeviceInformation.CreateFromIdAsync(outputDeviceId);

        // Safely change the output device of the player.
        // It's not allowed to change the audio device while the player is opening a media file.
        // We also don't want any exception raised here crashes the app.
        try
        {
            if (_player.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Opening)
            {
                _mediaOpenCompletionSource = new TaskCompletionSource<bool>();
                await _mediaOpenCompletionSource.Task;
                _player.AudioDevice = deviceInformation;
            }
            else
            {
                _player.AudioDevice = deviceInformation;
            }
        }
        catch (Exception e)
        {
            _telemetry.TrackError(e, new Dictionary<string, string>
            {
                { "outputDeviceId", outputDeviceId }
            });

            _mediaOpenCompletionSource?.TrySetException(e);
        }

        _lastUsedOutputDeviceId = outputDeviceId;
        _outputDeviceLock.Release();
    }

    private void OnMediaOpened(MediaPlayer sender, object args)
    {
        _mediaOpenCompletionSource?.TrySetResult(true);
    }

    private void OnMediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
    {
        _mediaOpenCompletionSource?.TrySetResult(false);
    }

    private void OnUserSettingsSet(object sender, string e)
    {
        // If the selected output device is changed in settings, updates the player's property to use the new device.
        if (e == UserSettingsConstants.OutputAudioDeviceId)
        {
            SetOutputDevice();
        }
    }

    private void OnAudioDeviceChanged(object sender, DefaultAudioRenderDeviceChangedEventArgs args)
    {
        // Gets and uses the new output device when the default audio device is changed.
        SetOutputDevice();
    }
}
