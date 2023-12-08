using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Models;
using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class MixMediaPlayerService : IMixMediaPlayerService
{
    private const double DefaultFadeInDurationMs = 1000;
    private const double DefaultFadeOutDurationMs = 300;

    private readonly Dictionary<string, IMediaPlayer> _activePlayers = new();
    private readonly Dictionary<string, string> _soundNames = new();
    private readonly Dictionary<string, DateTimeOffset> _activeSoundDateTimes = new();
    private readonly ISystemMediaControls _smtc;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly ISoundService _soundDataProvider;
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly IMediaPlayerFactory _mediaPlayerFactory;
    private readonly IUserSettings _userSettings;
    private readonly int _maxActive;
    private readonly string _localDataFolderPath;
    private (string GuideId, IMediaPlayer GuidePlayer)? _guideInfo;
    private double _globalVolume;
    private MediaPlaybackState _playbackState = MediaPlaybackState.Paused;

    /// <inheritdoc/>
    public event EventHandler<SoundPlayedArgs>? SoundAdded;

    /// <inheritdoc/>
    public event EventHandler<SoundPausedArgs>? SoundRemoved;

    /// <inheritdoc/>
    public event EventHandler<MixPlayedArgs>? MixPlayed;

    /// <inheritdoc/>
    public event EventHandler<MediaPlaybackState>? PlaybackStateChanged;

    /// <inheritdoc/>
    public event EventHandler<TimeSpan>? GuidePositionChanged;

    public MixMediaPlayerService(
        IUserSettings userSettings,
        ISoundService soundDataProvider,
        IAssetLocalizer assetLocalizer,
        IDispatcherQueue dispatcherQueue,
        IMediaPlayerFactory mediaPlayerFactory,
        ISystemInfoProvider systemInfoProvider,
        ISystemMediaControls systemMediaControls)
    {
        _userSettings = userSettings;
        _soundDataProvider = soundDataProvider;
        _assetLocalizer = assetLocalizer;
        _maxActive = userSettings.Get<int>(UserSettingsConstants.MaxActive);
        _dispatcherQueue = dispatcherQueue;
        _mediaPlayerFactory = mediaPlayerFactory;
        _localDataFolderPath = systemInfoProvider.LocalFolderPath();
        _smtc = systemMediaControls;
        InitializeSmtc();
    }

    /// <inheritdoc/>
    public Dictionary<string, string[]> Screensavers { get; } = new();

    /// <inheritdoc/>
    public string CurrentMixId { get; set; } = "";

    /// <inheritdoc/>
    public TimeSpan GuideDuration => _guideInfo?.GuidePlayer.Duration ?? TimeSpan.MinValue;

    /// <inheritdoc/>
    public string CurrentGuideId => _guideInfo?.GuideId ?? string.Empty;

    /// <inheritdoc/>
    public double GlobalVolume
    {
        get => _globalVolume;
        set => UpdateAllVolumes(value);
    }

    /// <inheritdoc/>
    public MediaPlaybackState PlaybackState
    {
        get => _playbackState;
        set
        {
            _playbackState = value;
            PlaybackStateChanged?.Invoke(this, value);

            if (value == MediaPlaybackState.Playing)
                _smtc.PlaybackStatus = SystemMediaState.Playing;
            else if (value == MediaPlaybackState.Paused)
                _smtc.PlaybackStatus = SystemMediaState.Paused;
        }
    }

    /// <inheritdoc/>
    public void SetMixId(string mixId)
    {
        if (string.IsNullOrWhiteSpace(mixId))
        {
            return;
        }

        CurrentMixId = mixId;
        MixPlayed?.Invoke(this, new MixPlayedArgs(mixId, _activePlayers.Keys.ToArray()));
    }

    private void UpdateAllVolumes(double value)
    {
        if (value < 0d || value > 1d)
        {
            return;
        }

        if (value == 0d)
        {
            // prevent volume from being permanently zero.
            value = 0.000001d;
        }

        foreach (var soundId in _activePlayers.Keys)
        {
            _activePlayers[soundId].Volume = GetVolume(soundId) * value;
        }

        if (_guideInfo?.GuidePlayer is IMediaPlayer guidePlayer)
        {
            guidePlayer.Volume = value;
        }

        // Must be set last since GetVolume
        // uses the old global volume.
        _globalVolume = value;
    }

    /// <inheritdoc/>
    public bool IsSoundPlaying(string soundId)
    {
        if (string.IsNullOrEmpty(soundId))
        {
            return false;
        }

        return _activePlayers.ContainsKey(soundId) || _guideInfo?.GuideId == soundId;
    }

    /// <inheritdoc/>
    public async Task PlayRandomAsync()
    {
        RemoveAll();
        var sound = await _soundDataProvider.GetRandomSoundAsync();
        if (sound is not null)
        {
            await ToggleSoundAsync(sound);
        }
    }

    public async Task AddRandomAsync()
    {
        if (GetSoundIds().Length >= _maxActive)
        {
            return;
        }

        var sound = await _soundDataProvider.GetRandomSoundAsync();
        if (sound is not null)
        {
            await ToggleSoundAsync(sound);
        }
    }

    /// <inheritdoc/>
    public string[] GetSoundIds() => _activePlayers.Keys.ToArray();

    public async Task PlayGuideAsync(Guide guide)
    {
        if (_guideInfo?.GuideId == guide.Id)
        {
            // already loaded so don't change anything. Just play it
            Play();
            return;
        }

        if (!guide.IsDownloaded)
        {
            ThrowHelper.ThrowArgumentException("The guide you tried to play wasn't downloaded. " +
                "This should never happen. If it does, then you're doing something wrong. Please fix.");
        }

        IMediaPlayer player = _guideInfo?.GuidePlayer
            ?? _mediaPlayerFactory.CreatePlayer(disableDefaultSystemControls: true);

        player.Pause();
        bool success = await player.SetSourceAsync(guide.FilePath);
        
        if (success)
        {
            player.PositionChanged -= OnGuidePositionChanged;
            player.PositionChanged += OnGuidePositionChanged;

            // TODO  refresh smtc title
            player.Volume = _globalVolume;

            _guideInfo = (guide.Id, player);
            Play();
        }
    }

    private void OnGuidePositionChanged(object sender, TimeSpan e)
    {
        GuidePositionChanged?.Invoke(sender, e);
    }

    /// <inheritdoc/>
    public async Task ToggleSoundAsync(Sound sound, bool keepPaused = false, string parentMixId = "")
    {
        if (string.IsNullOrWhiteSpace(sound?.Id))
        {
            return;
        }

        if (IsSoundPlaying(sound!.Id))
        {
            return;
        }

        if (_activePlayers.Count >= _maxActive)
        {
            // remove sound
            var oldestTime = _activeSoundDateTimes.Min(static x => x.Value);
            var oldestSoundId = _activeSoundDateTimes.FirstOrDefault(x => x.Value == oldestTime).Key;
            RemoveSound(oldestSoundId);
        }

        if (_activePlayers.Count < _maxActive)
        {
            IMediaPlayer player = _mediaPlayerFactory.CreatePlayer(disableDefaultSystemControls: true);
            bool sourceSetSuccessfully = false;

            if (Uri.IsWellFormedUriString(sound.FilePath, UriKind.Absolute))
            {
                // sound path is packaged and must be read as URI.
                sourceSetSuccessfully = player.SetUriSource(new Uri(sound.FilePath), enableGaplessLoop: true);
            }
            else if (sound.FilePath is not null && sound.FilePath.Contains(_localDataFolderPath))
            {
                sourceSetSuccessfully = await player.SetSourceAsync(sound.FilePath, enableGaplessLoop: true);
            }

            if (sourceSetSuccessfully)
            {
                CurrentMixId = parentMixId;
                player.Volume *= _globalVolume;
                if (!_activePlayers.ContainsKey(sound.Id)) _activePlayers.Add(sound.Id, player);
                if (!_activeSoundDateTimes.ContainsKey(sound.Id)) _activeSoundDateTimes.Add(sound.Id, DateTimeOffset.Now);
                if (!Screensavers.ContainsKey(sound.Id) && sound.ScreensaverImagePaths is { Length: > 0 } images) Screensavers.Add(sound.Id, images);
                if (!_soundNames.ContainsKey(sound.Id)) _soundNames.Add(sound.Id, _assetLocalizer.GetLocalName(sound));
                RefreshSmtcTitle();

                if (keepPaused) Pause();
                else Play();

                SoundAdded?.Invoke(this, new SoundPlayedArgs(sound, parentMixId));
            }
        }
    }

    /// <inheritdoc/>
    public void SetVolume(string soundId, double value)
    {
        if (IsSoundPlaying(soundId) && value <= 1d && value >= 0d)
        {
            _activePlayers[soundId].Volume = value * _globalVolume;
        }
    }

    /// <inheritdoc/>
    public double GetVolume(string soundId)
    {
        if (IsSoundPlaying(soundId))
        {
            return _activePlayers[soundId].Volume / _globalVolume;
        }

        return 0;
    }

    public void Play()
    {
        PlaybackState = MediaPlaybackState.Playing;
        foreach (var key in _activePlayers.Keys)
        {
            double volume = _userSettings.Get($"{key}:volume", 100d) / 100;
            _activePlayers[key].Play(fadeInTargetVolume: volume * _globalVolume, fadeDuration: DefaultFadeInDurationMs);
        }

        _guideInfo?.GuidePlayer.Play(fadeInTargetVolume: 1.0 * _globalVolume, fadeDuration: DefaultFadeInDurationMs);
    }

    public void Pause()
    {
        PlaybackState = MediaPlaybackState.Paused;
        foreach (var p in _activePlayers.Values)
        {
            p.Pause(fadeDuration: DefaultFadeOutDurationMs);
        }
        _guideInfo?.GuidePlayer.Pause(fadeDuration: DefaultFadeOutDurationMs);
    }

    /// <inheritdoc/>
    public void RemoveAll()
    {
        foreach (var soundId in _activePlayers.Keys.ToList())
        {
            RemoveSound(soundId);
        }

        RemoveGuide();
    }

    /// <inheritdoc/>
    public void RemoveSound(string soundId)
    {
        if (string.IsNullOrWhiteSpace(soundId) || !IsSoundPlaying(soundId))
        {
            return;
        }

        if (_guideInfo?.GuideId == soundId)
        {
            RemoveGuide();
            return;
        }

        var player = _activePlayers[soundId];
        player.Pause(fadeDuration: DefaultFadeOutDurationMs, disposeAfterFadeOut: true);

        _activePlayers[soundId] = null!;
        _activePlayers.Remove(soundId);
        _activeSoundDateTimes.Remove(soundId);
        _soundNames.Remove(soundId);
        Screensavers.Remove(soundId);
        RefreshSmtcTitle();

        // Any time we remove a sound,
        // we are guaranteed to "destruct"
        // the previous active mix, so we clear
        // the mix id.
        var previousParentMixId = CurrentMixId;
        CurrentMixId = "";

        if (_activePlayers.Count == 0)
        {
            Pause();
        }

        SoundRemoved?.Invoke(this, new SoundPausedArgs(soundId, previousParentMixId));
    }

    /// <summary>
    /// Initializes the Sytem Media Transport Control
    /// integration. This is required in order to
    /// play background audio. And of course,
    /// we also want to integrate with the OS music controls.
    /// </summary>
    private void InitializeSmtc()
    {
        // ref: https://docs.microsoft.com/en-us/windows/uwp/audio-video-camera/system-media-transport-controls
        _smtc.IsPlayEnabled = true;
        _smtc.IsPauseEnabled = true;
        _smtc.IsNextEnabled = false;
        _smtc.IsPreviousEnabled = false;
        _smtc.ButtonPressed += OnButtonPressed;
        RefreshSmtcTitle();
    }

    private void OnButtonPressed(object sender, SystemMediaControlsButton e)
    {
        // Note: Playing and pausing the players
        // will not work unless we move to the UI thread.
        // Thus we use the dispatcher queue below.

        switch (e)
        {
            case SystemMediaControlsButton.Play:
                _dispatcherQueue.TryEnqueue(Play);
                break;
            case SystemMediaControlsButton.Pause:
                _dispatcherQueue.TryEnqueue(Pause);
                break;
            default:
                break;
        }
    }

    private void RefreshSmtcTitle()
    {
        var title = _soundNames.Count == 0 ? "Ambie" : string.Join(" / ", _soundNames.Values);
        _smtc.UpdateDisplay(title, "Ambie");
    }

    private void RemoveGuide()
    {
        if (_guideInfo is { } guideInfo)
        {
            guideInfo.GuidePlayer.Pause(fadeDuration: DefaultFadeOutDurationMs, disposeAfterFadeOut: true);
            guideInfo.GuidePlayer.PositionChanged -= OnGuidePositionChanged;
        }

        _guideInfo = null;
    }
}