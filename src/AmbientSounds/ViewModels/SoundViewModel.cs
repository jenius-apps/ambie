using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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
        private readonly ISoundMixService _soundMixService;
        private readonly ITelemetry _telemetry;
        private readonly IRenamer _renamer;
        private readonly IDialogService _dialogService;
        private readonly IIapService _iapService;
        private readonly IDownloadManager _downloadManager;
        private Progress<double>? _downloadProgress;
        private int _position;
        private int _setSize;
        private bool _downloadActive;
        private double _downloadProgressValue;

        public SoundViewModel(
            Sound s,
            IMixMediaPlayerService playerService,
            ISoundDataProvider soundDataProvider,
            ISoundMixService soundMixService,
            ITelemetry telemetry,
            IRenamer renamer,
            IDialogService dialogService,
            IIapService iapService,
            IDownloadManager downloadManager)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsNotNull(playerService, nameof(playerService));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(soundMixService, nameof(soundMixService));
            Guard.IsNotNull(renamer, nameof(renamer));
            Guard.IsNotNull(dialogService, nameof(dialogService));
            Guard.IsNotNull(iapService, nameof(iapService));
            Guard.IsNotNull(downloadManager, nameof(downloadManager));

            _sound = s;
            _soundMixService = soundMixService;
            _playerService = playerService;
            _soundDataProvider = soundDataProvider;
            _telemetry = telemetry;
            _renamer = renamer;
            _dialogService = dialogService;
            _iapService = iapService;
            _downloadManager = downloadManager;

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

        /// <summary>
        /// The sound's GUID.
        /// </summary>
        public string? Id => _sound.Id;

        /// <summary>
        /// Determines if the plus badge is visible.
        /// </summary>
        public bool PlusBadgeVisible => _sound.IsPremium && _sound.IapId == IapConstants.MsStoreAmbiePlusId;

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

        public void Initialize()
        {
            _playerService.SoundRemoved += OnSoundPaused;
            _playerService.SoundAdded += OnSoundPlayed;
            _playerService.MixPlayed += OnMixPlayed;

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

            if (_sound.IsPremium && _sound.IapId == IapConstants.MsStoreAmbiePlusId)
            {
                var owned = await _iapService.IsOwnedAsync(_sound.IapId);
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
            }
        }

        private void OnSoundPaused(object sender, SoundPausedArgs args)
        {
            OnPropertyChanged(nameof(IsCurrentlyPlaying));
        }

        private void OnSoundPlayed(object sender, SoundPlayedArgs args)
        {
            if (args.ParentMixId == _sound.Id || args.Sound.Id == _sound.Id)
            {
                OnPropertyChanged(nameof(IsCurrentlyPlaying));
            }
        }

        private async void DeleteSound()
        {
            if (!_sound.IsMix)
            {
                _playerService.RemoveSound(_sound.Id);
            }

            await _soundDataProvider.DeleteLocalSoundAsync(_sound.Id ?? "");

            _telemetry.TrackEvent(TelemetryConstants.DeleteClicked, new Dictionary<string, string>
            {
                { "name", _sound.Name ?? "" },
                { "id", _sound.Id ?? "" }
            });
        }

        public void Dispose()
        {
            _playerService.SoundRemoved -= OnSoundPaused;
            _playerService.SoundAdded -= OnSoundPlayed;
            _playerService.MixPlayed -= OnMixPlayed;

            if (_downloadProgress is not null)
            {
                _downloadProgress.ProgressChanged -= OnProgressChanged;
            }
        }
    }
}
