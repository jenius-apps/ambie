using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    public class MixMediaPlayerService : IMixMediaPlayerService
    {
        private readonly Dictionary<string, MediaPlayer> _activePlayers = new();
        private readonly Dictionary<string, DateTimeOffset> _activeSoundDateTimes = new();
        private readonly int _maxActive;
        private double _globalVolume;
        private MediaPlaybackState _playbackState = MediaPlaybackState.Paused;
        private readonly SystemMediaTransportControls _smtc;
        private readonly DispatcherQueue _dispatcherQueue;

        /// <inheritdoc/>
        public event EventHandler<SoundPlayedArgs>? SoundAdded;

        /// <inheritdoc/>
        public event EventHandler<SoundPausedArgs>? SoundRemoved;

        /// <inheritdoc/>
        public event EventHandler<MixPlayedArgs>? MixPlayed;

        /// <inheritdoc/>
        public event EventHandler<MediaPlaybackState>? PlaybackStateChanged;

        public MixMediaPlayerService(IUserSettings userSettings)
        {
            _maxActive = userSettings?.Get<int>(UserSettingsConstants.MaxActive) ?? 3;
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            _smtc = SystemMediaTransportControls.GetForCurrentView();
            InitializeSmtc();
        }

        private void SmtcButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            // Note: Playing and pausing the players
            // will not work unless we move to the UI thread.
            // Thus we use the dispatcher queue below.

            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play:
                    _dispatcherQueue.TryEnqueue(Play);
                    break;
                case SystemMediaTransportControlsButton.Pause:
                    _dispatcherQueue.TryEnqueue(Pause);
                    break;
                default:
                    break;
            }
        }

        /// <inheritdoc/>
        public Dictionary<string, string[]> Screensavers { get; } = new();

        /// <inheritdoc/>
        public string CurrentMixId { get; set; } = "";

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
                    _smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
                else if (value == MediaPlaybackState.Paused) 
                    _smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
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

            // Must be set last since GetVolume
            // uses the old global volume.
            _globalVolume = value;
        }

        /// <inheritdoc/>
        public bool IsSoundPlaying(string soundId)
        {
            return !string.IsNullOrWhiteSpace(soundId) && _activePlayers.ContainsKey(soundId);
        }

        /// <inheritdoc/>
        public string[] GetSoundIds() => _activePlayers.Keys.ToArray();

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
                MediaSource? mediaSource = null;
                if (Uri.IsWellFormedUriString(sound.FilePath, UriKind.Absolute))
                {
                    // sound path is packaged and can be read as URI.
                    mediaSource = MediaSource.CreateFromUri(new Uri(sound.FilePath));
                }
                else if (sound.FilePath is not null && sound.FilePath.Contains(ApplicationData.Current.LocalFolder.Path))
                {
                    try
                    {
                        // sound path is a file saved in local folder
                        StorageFile file = await StorageFile.GetFileFromPathAsync(sound.FilePath);
                        mediaSource = MediaSource.CreateFromStorageFile(file);
                    }
                    catch
                    {
                        // todo log
                        return;
                    }
                }
                
                if (mediaSource is not null)
                {
                    CurrentMixId = parentMixId;
                    var player = CreateLoopingPlayer();
                    player.Volume *= _globalVolume;
                    player.Source = mediaSource;
                    _activePlayers.TryAdd(sound.Id, player);
                    _activeSoundDateTimes.TryAdd(sound.Id, DateTimeOffset.Now);
                    Screensavers.TryAdd(sound.Id, sound.ScreensaverImagePaths ?? Array.Empty<string>());

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
            foreach (var p in _activePlayers.Values)
            {
                p.Play();
            }
        }

        public void Pause()
        {
            PlaybackState = MediaPlaybackState.Paused;
            foreach (var p in _activePlayers.Values)
            {
                p.Pause();
            }
        }

        /// <inheritdoc/>
        public void RemoveAll()
        {
            foreach (var soundId in _activePlayers.Keys.ToList())
            {
                RemoveSound(soundId);
            }
        }

        /// <inheritdoc/>
        public IList<string> GetActiveIds()
        {
            return _activePlayers.Keys.ToArray();
        }

        /// <inheritdoc/>
        public void RemoveSound(string soundId)
        {
            if (string.IsNullOrWhiteSpace(soundId) || !IsSoundPlaying(soundId))
            {
                return;
            }

            var player = _activePlayers[soundId];
            player.Pause();
            player.Dispose();

            _activePlayers[soundId] = null!;
            _activePlayers.Remove(soundId);
            _activeSoundDateTimes.Remove(soundId);
            Screensavers.Remove(soundId);

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
            _smtc.ButtonPressed += SmtcButtonPressed;
        }

        private MediaPlayer CreateLoopingPlayer()
        {
            var player = new MediaPlayer()
            {
                IsLoopingEnabled = true,
            };

            player.CommandManager.IsEnabled = false;
            return player;
        } 
    }
}
