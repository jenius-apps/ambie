using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using AmbientSounds.Tools;

namespace AmbientSounds.ViewModels
{
    /// <summary>
    /// View model for a sound object.
    /// </summary>
    public class SoundViewModel : ObservableObject
    {
        private readonly Sound _sound;
        private readonly IMixMediaPlayerService _playerService;
        private readonly ISoundDataProvider _soundDataProvider;
        private readonly ISoundService _soundService;
        private readonly IOnlineSoundDataProvider _onlineSoundDataProvider;
        private readonly ISoundMixService _soundMixService;
        private readonly ITelemetry _telemetry;
        private readonly IRenamer _renamer;
        private readonly IDialogService _dialogService;
        private readonly IIapService _iapService;
        private readonly IDownloadManager _downloadManager;
        private readonly IPresenceService _presenceService;
        private readonly IDispatcherQueue _dispatcherQueue;
        private Progress<double>? _downloadProgress;
        private int _position;
        private int _setSize;
        private bool _downloadActive;
        private double _downloadProgressValue;
        private double _presenceCount = 0;

        public SoundViewModel(
            Sound s,
            IMixMediaPlayerService playerService,
            ISoundDataProvider soundDataProvider,
            ISoundService soundService,
            ISoundMixService soundMixService,
            ITelemetry telemetry,
            IRenamer renamer,
            IDialogService dialogService,
            IIapService iapService,
            IDownloadManager downloadManager,
            IPresenceService presenceService,
            IDispatcherQueue dispatcherQueue,
            IOnlineSoundDataProvider onlineSoundDataProvider)
        {
            Guard.IsNotNull(s);
            Guard.IsNotNull(playerService);
            Guard.IsNotNull(soundDataProvider);
            Guard.IsNotNull(soundService);
            Guard.IsNotNull(telemetry);
            Guard.IsNotNull(soundMixService);
            Guard.IsNotNull(renamer);
            Guard.IsNotNull(dialogService);
            Guard.IsNotNull(iapService);
            Guard.IsNotNull(downloadManager);
            Guard.IsNotNull(presenceService);
            Guard.IsNotNull(dispatcherQueue);
            Guard.IsNotNull(onlineSoundDataProvider);

            _sound = s;
            _soundMixService = soundMixService;
            _playerService = playerService;
            _soundDataProvider = soundDataProvider;
            _soundService = soundService;
            _telemetry = telemetry;
            _renamer = renamer;
            _dialogService = dialogService;
            _iapService = iapService;
            _downloadManager = downloadManager;
            _presenceService = presenceService;
            _dispatcherQueue = dispatcherQueue;
            _onlineSoundDataProvider = onlineSoundDataProvider;

            DeleteCommand = new RelayCommand(DeleteSound);
            RenameCommand = new AsyncRelayCommand(RenameAsync);
            PlayCommand = new AsyncRelayCommand(PlayAsync);
        }

        public int Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public int SetSize
        {
            get => _setSize;
            set => SetProperty(ref _setSize, value);
        }

        public IAsyncRelayCommand<IList<string>>? MixUnavailableCommand { get; set; }

        /// <summary>
        /// The sound's GUID.
        /// </summary>
        public string? Id => _sound.Id;

        /// <summary>
        /// Determines if the plus badge is visible.
        /// </summary>
        public bool PlusBadgeVisible
        {
            get
            {
                if (_sound.IapIds.Count > 0)
                {
                    return _sound.IsPremium && _sound.IapIds.ContainsAmbiePlus() && !_sound.IapIds.ContainsFreeId();
                }
                else
                {
                    // backwards compatibility
                    return _sound.IsPremium && _sound.IapId == IapConstants.MsStoreAmbiePlusId;
                }
            }
        }

        /// <summary>
        /// Determines if the free badge is visible
        /// </summary>
        public bool FreeBadgeVisible => _sound.IsPremium && _sound.IapIds.ContainsFreeId();

        /// <summary>
        /// The sound's attribution.
        /// </summary>
        public string? Attribution => _sound.Attribution;

        /// <summary>
        /// Name of the sound.
        /// </summary>
        public string? Name => _sound.Name;

        /// <summary>
        /// True if the sound is a mix.
        /// </summary>
        public bool IsMix => _sound.IsMix;

        public bool IsNotMix => !IsMix;

        public string ColourHex => _sound.ColourHex;

        public bool HasSecondImage => IsMix && _sound.ImagePaths.Length == 2;

        public string? SecondImagePath => _sound.ImagePaths.Length >= 2 ? _sound.ImagePaths[1] : "http://localhost:8000";

        public bool HasThirdImage => IsMix && _sound.ImagePaths.Length == 3;

        public string? ThirdImagePath => _sound.ImagePaths.Length >= 3 ? _sound.ImagePaths[2] : "http://localhost:8000";

        /// <summary>
        /// The path for the image to display for the current sound.
        /// </summary>
        public string? ImagePath => _sound.IsMix ? _sound.ImagePaths[0] : _sound.ImagePath;

        /// <summary>
        /// If true, item can be deleted from local storage.
        /// </summary>
        public bool CanDelete => !_sound.FilePath?.StartsWith("ms-appx") ?? false;

        /// <summary>
        /// Command for deleting this sound.
        /// </summary>
        public IRelayCommand DeleteCommand { get; }

        public IAsyncRelayCommand RenameCommand { get; }

        public IAsyncRelayCommand PlayCommand { get; }

        /// <summary>
        /// Returns true if the sound is currently playing.
        /// </summary>
        public bool IsCurrentlyPlaying => string.IsNullOrWhiteSpace(_playerService.CurrentMixId)
            ? _playerService.IsSoundPlaying(_sound.Id)
            : _soundMixService.IsMixPlaying(_sound.Id);

        public bool DownloadActive
        {
            get => _downloadActive;
            set => SetProperty(ref _downloadActive, value);
        }

        public double DownloadProgressValue
        {
            get => _downloadProgressValue;
            set => SetProperty(ref _downloadProgressValue, value);
        }

        public double PresenceCount
        {
            get => _presenceCount;
            set
            {
                SetProperty(ref _presenceCount, value);
                OnPropertyChanged(nameof(IsPresenceVisible));
            }
        }

        public bool IsPresenceVisible => PresenceCount > 0 && IsCurrentlyPlaying;

        public void Initialize()
        {
            _playerService.SoundRemoved += OnSoundPaused;
            _playerService.SoundAdded += OnSoundPlayed;
            _playerService.MixPlayed += OnMixPlayed;
            _presenceService.SoundPresenceChanged += OnPresenceChanged;
            _presenceService.PresenceDisconnected += OnPresenceDisconnected;

            DownloadActive = _downloadManager.IsDownloadActive(_sound);
            if (DownloadActive)
            {
                var progress = _downloadManager.GetProgress(_sound);
                if (progress is not null)
                {
                    RegisterProgress(progress);
                }
            }

        }

        private void RegisterProgress(IProgress<double> progress)
        {
            if (progress is Progress<double> p)
            {
                if (_downloadProgress is not null)
                {
                    _downloadProgress.ProgressChanged -= OnProgressChanged;
                }

                _downloadProgress = p;
                _downloadProgress.ProgressChanged += OnProgressChanged;
            }
        }

        private void OnProgressChanged(object sender, double e)
        {
            DownloadProgressValue = e;
            if (e >= 100)
            {
                DownloadActive = false;
                DownloadProgressValue = 0;
            }
        }

        /// <summary>
        /// Loads this sound into the player and plays it.
        /// </summary>
        private async Task PlayAsync()
        {
            if (DownloadActive)
            {
                return;
            }

            if (IsCurrentlyPlaying)
            {
                _playerService.RemoveSound(_sound.Id);
                return;
            }

            if (_sound.IsPremium)
            {
                if (_sound.IapIds.ContainsFreeId())
                {
                    bool stillFree;
                    try
                    {
                        var items = await _onlineSoundDataProvider.GetOnlineSoundsAsync(
                            new string[] { _sound.Id },
                            IapConstants.MsStoreFreeRotationId);
                        stillFree = items.Count >= 1;
                    }
                    catch (Exception e)
                    {
                        // if we don't know what happened, assume it's still free.
                        stillFree = true;
                        _telemetry.TrackError(e);
                    }

                    if (!stillFree)
                    {
                        var newList = new List<string>(_sound.IapIds);
                        newList.Remove(IapConstants.MsStoreFreeRotationId);
                        _sound.IapIds = newList;
                        OnPropertyChanged(nameof(FreeBadgeVisible));
                        OnPropertyChanged(nameof(PlusBadgeVisible));
                        _ = _soundDataProvider.UpdateLocalSoundAsync(new Sound[] { _sound }).ConfigureAwait(false);

                        _telemetry.TrackEvent(TelemetryConstants.ExpiredClicked, new Dictionary<string, string>
                        {
                            { "name", Name ?? "" },
                        });
                    }
                    else
                    {
                        _telemetry.TrackEvent(TelemetryConstants.FreeClicked, new Dictionary<string, string>
                        {
                            { "name", Name ?? "" },
                        });
                    }
                }

                var owned = _sound.IapIds.Count > 0
                    ? await _iapService.IsAnyOwnedAsync(_sound.IapIds)
                    : await _iapService.IsOwnedAsync(_sound.IapId); // backwards compatibility

                if (!owned)
                {
                    await _dialogService.OpenPremiumAsync();
                    return;
                }
            }

            if (!IsMix)
            {
                await _playerService.ToggleSoundAsync(_sound);
            }
            else
            {
                IEnumerable<string> unavailable = await _soundMixService.GetUnavailableSoundsAsync(_sound);
                if (unavailable.Any())
                {
                    if (MixUnavailableCommand is not null)
                    {
                        await MixUnavailableCommand.ExecuteAsync(unavailable.ToArray());
                    }
                    return;
                }

                await _soundMixService.LoadMixAsync(_sound);
            }

            _telemetry.TrackEvent(TelemetryConstants.SoundClicked, new Dictionary<string, string>
            {
                { "id", Name ?? "" },
                { "mix", IsMix.ToString() }
            });
        }

        private async Task RenameAsync()
        {
            bool renamed = await _renamer.RenameAsync(_sound);

            if (renamed)
            {
                OnPropertyChanged(nameof(Name));
            }
        }

        private void OnMixPlayed(object sender, MixPlayedArgs args)
        {
            if (args.MixId == _sound.Id || args.SoundIds.Contains(_sound.Id))
            {
                OnPropertyChanged(nameof(IsCurrentlyPlaying));
                OnPropertyChanged(nameof(IsPresenceVisible));
            }
        }

        private void OnPresenceChanged(object sender, PresenceEventArgs e)
        {
            if (e.SoundId == _sound.Id)
            {
                _dispatcherQueue.TryEnqueue(() =>
                {
                    PresenceCount = e.Count;
                });
            }
        }

        private void OnPresenceDisconnected(object sender, EventArgs e)
        {
            _dispatcherQueue.TryEnqueue(() =>
            { 
                PresenceCount = 0; 
            });
        }

        private void OnSoundPaused(object sender, SoundPausedArgs args)
        {
            OnPropertyChanged(nameof(IsCurrentlyPlaying));
            OnPropertyChanged(nameof(IsPresenceVisible));
        }

        private void OnSoundPlayed(object sender, SoundPlayedArgs args)
        {
            if (args.ParentMixId == _sound.Id || args.Sound.Id == _sound.Id)
            {
                OnPropertyChanged(nameof(IsCurrentlyPlaying));
                OnPropertyChanged(nameof(IsPresenceVisible));
            }
        }

        private async void DeleteSound()
        {
            if (!_sound.IsMix)
            {
                _playerService.RemoveSound(_sound.Id);
            }

            try
            {
                await _soundService.DeleteLocalSoundAsync(_sound.Id ?? "");

                _telemetry.TrackEvent(TelemetryConstants.DeleteClicked, new Dictionary<string, string>
                {
                    { "name", _sound.Name ?? "" },
                    { "id", _sound.Id ?? "" }
                });
            }
            catch { }
        }

        public void Dispose()
        {
            _playerService.SoundRemoved -= OnSoundPaused;
            _playerService.SoundAdded -= OnSoundPlayed;
            _playerService.MixPlayed -= OnMixPlayed;
            _presenceService.SoundPresenceChanged -= OnPresenceChanged;
            _presenceService.PresenceDisconnected -= OnPresenceDisconnected;

            if (_downloadProgress is not null)
            {
                _downloadProgress.ProgressChanged -= OnProgressChanged;
            }
        }
    }
}
