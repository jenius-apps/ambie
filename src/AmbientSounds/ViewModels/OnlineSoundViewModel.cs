using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class OnlineSoundViewModel : ObservableObject
    {
        private readonly Sound _sound;
        private readonly IDownloadManager _downloadManager;
        private readonly ISoundDataProvider _soundDataProvider;
        private readonly ITelemetry _telemetry;
        private readonly IIapService _iapService;
        private readonly IPreviewService _previewService;
        private readonly IDialogService _dialogService;
        private Progress<double> _downloadProgress;
        private double _progressValue;
        private bool _isInstalled;
        private bool _isOwned;
        private string _price = "";

        public OnlineSoundViewModel(
            Sound s, 
            IDownloadManager downloadManager,
            ISoundDataProvider soundDataProvider,
            ITelemetry telemetry,
            IPreviewService previewService,
            IIapService iapService,
            IDialogService dialogService)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsNotNull(downloadManager, nameof(downloadManager));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(iapService, nameof(iapService));
            Guard.IsNotNull(previewService, nameof(previewService));
            Guard.IsNotNull(dialogService, nameof(dialogService));
            _sound = s;
            _downloadManager = downloadManager;
            _previewService = previewService;
            _iapService = iapService;
            _soundDataProvider = soundDataProvider;
            _telemetry = telemetry;
            _dialogService = dialogService;

            _downloadProgress = new Progress<double>();
            _downloadProgress.ProgressChanged += OnProgressChanged;
            _soundDataProvider.LocalSoundDeleted += OnSoundDeleted;
            _iapService.ProductPurchased += OnProductPurchased;

            DownloadCommand = new AsyncRelayCommand(DownloadAsync);
            LoadCommand = new AsyncRelayCommand(LoadAsync);
            DeleteCommand = new AsyncRelayCommand(DeleteSound);
            BuyCommand = new AsyncRelayCommand(BuySoundAsync);
            PreviewCommand = new RelayCommand(Preview);
        }

        private async void OnProductPurchased(object sender, string iapId)
        {
            if (!_sound.IsPremium || _sound.IapId != iapId)
            {
                return;
            }

            IsOwned = await _iapService.IsOwnedAsync(_sound.IapId);
        }

        private async void OnSoundDeleted(object sender, string id)
        {
            if (id == _sound.Id)
            {
                IsInstalled = await _soundDataProvider.IsSoundInstalledAsync(_sound.Id);
                DownloadProgressValue = 0;

                // Note: a non-premium sound is treated as "owned"
                IsOwned = _sound.IsPremium ? await _iapService.IsOwnedAsync(_sound.IapId) : true;
            }
        }

        private async void OnProgressChanged(object sender, double e)
        {
            DownloadProgressValue = e;
            if (e >= 100)
            {
                IsInstalled = await _soundDataProvider.IsSoundInstalledAsync(_sound.Id ?? "");
                DownloadProgressValue = 0;
            }
        }

        /// <summary>
        /// Gets or sets the location within the application of this viewmodel.
        /// This is used for telemetry purposes.
        /// </summary>
        public string TelemetryLocation { get; set; } = "catalogue";

        /// <summary>
        /// The sound's attribution.
        /// </summary>
        public string? Attribution => _sound.Attribution;

        /// <summary>
        /// Name of the sound.
        /// </summary>
        public string? Name => _sound.Name;

        public string ColourHex => _sound.ColourHex;

        /// <summary>
        /// Id of the sound.
        /// </summary>
        public string Id => _sound.Id;

        /// <summary>
        /// The path for the image to display for the current sound.
        /// </summary>
        public string ImagePath => _sound.ImagePath;

        /// <summary>
        /// Array of sponsor links provided by sound author
        /// to be displayed on screen if the links are valid.
        /// </summary>
        public string[] ValidSponsorLinks => AreLinksValid
            ? _sound.SponsorLinks.Where(static x => Uri.IsWellFormedUriString(x, UriKind.Absolute)).ToArray()
            : Array.Empty<string>();

        /// <summary>
        /// Determines if it's safe to display the links.
        /// </summary>
        public bool AreLinksValid => _sound.SponsorLinks is not null && 
            _sound.SponsorLinks.Length > 0 && 
            _sound.SponsorLinks.Any(static x => Uri.IsWellFormedUriString(x, UriKind.Absolute));

        /// <summary>
        /// Determines if the sound can be previewed.
        /// </summary>
        public bool CanPreview => 
            !string.IsNullOrWhiteSpace(_sound.PreviewFilePath) && 
            Uri.IsWellFormedUriString(_sound.PreviewFilePath, UriKind.Absolute);

        /// <summary>
        /// If true, the sound is owned by the user
        /// and can be downloaded. If false, the sound
        /// must be purchased first.
        /// </summary>
        public bool IsOwned
        {
            get => _isOwned;
            set 
            { 
                SetProperty(ref _isOwned, value);
                OnPropertyChanged(nameof(CanBuy));
                OnPropertyChanged(nameof(DownloadButtonVisible));
            }
        }

        /// <summary>
        /// Determines if the download button is visible.
        /// </summary>
        public bool DownloadButtonVisible => IsOwned && !DownloadProgressVisible;

        /// <summary>
        /// Price of the item if it is premium.
        /// </summary>
        public string Price
        {
            get => _price;
            set => SetProperty(ref _price, value);
        }

        /// <summary>
        /// True if the sound can be bought.
        /// </summary>
        public bool CanBuy => _sound.IsPremium && !_isOwned;

        public bool PlusBadgeVisible => _sound.IsPremium;

        /// <summary>
        /// This sound's download progress.
        /// </summary>
        public double DownloadProgressValue
        {
            get => _progressValue;
            set
            {
                SetProperty(ref _progressValue, value);
                OnPropertyChanged(nameof(DownloadProgressVisible));
                OnPropertyChanged(nameof(DownloadButtonVisible));
            }
        }

        /// <summary>
        /// True if download progress should be visible.
        /// </summary>
        public bool DownloadProgressVisible => DownloadProgressValue > 0 && DownloadProgressValue < 100;

        /// <summary>
        /// True if the item is already installed.
        /// </summary>
        public bool IsInstalled
        {
            get => _isInstalled;
            set
            {
                SetProperty(ref _isInstalled, value);
                OnPropertyChanged(nameof(NotInstalled));
            }
        }

        /// <summary>
        /// True if the item can be downloaded;
        /// </summary>
        public bool NotInstalled => !IsInstalled;

        /// <summary>
        /// Command for downloading this sound.
        /// </summary>
        public IAsyncRelayCommand DownloadCommand { get; }

        /// <summary>
        /// Command for deleting this sound.
        /// </summary>
        public IAsyncRelayCommand DeleteCommand { get; }

        /// <summary>
        /// Command for buying sound.
        /// </summary>
        public IAsyncRelayCommand BuyCommand { get; }

        /// <summary>
        /// Command for loading this sound.
        /// </summary>
        public IAsyncRelayCommand LoadCommand { get; }

        /// <summary>
        /// Command for previewing this sound.
        /// </summary>
        public IRelayCommand PreviewCommand { get; }

        private void Preview()
        {
            _telemetry.TrackEvent(TelemetryConstants.PreviewClicked, new Dictionary<string, string>
            {
                { "id", _sound.Id ?? "" },
            });

            _previewService.Play(_sound.PreviewFilePath);
        }

        private async Task BuySoundAsync()
        {
            await _dialogService.OpenPremiumAsync();

            _telemetry.TrackEvent(TelemetryConstants.BuyClicked, new Dictionary<string, string>
            {
                { "id", _sound.Id },
                { "name", _sound.Name }
            });
        }

        private async Task DeleteSound()
        {
            _telemetry.TrackEvent(TelemetryConstants.CatalogueDeleteClicked, new Dictionary<string, string>
            {
                { "id", _sound.Id ?? "" },
            });
            await _soundDataProvider.DeleteLocalSoundAsync(_sound.Id ?? "");
        }

        private async Task LoadAsync()
        {
            if (_downloadManager.IsDownloadActive(_sound))
            {
                var progress = _downloadManager.GetProgress(_sound);
                if (progress is not null)
                {
                    RegisterProgress(progress);
                }
            }
            else
            {
                IsInstalled = await _soundDataProvider.IsSoundInstalledAsync(_sound.Id ?? "");
            }

            // Determine ownership
            bool isOwned;
            if (_sound.IsPremium)
            {
                isOwned = await _iapService.IsOwnedAsync(_sound.IapId);
                Price = await _iapService.GetPriceAsync(_sound.IapId);

                if (!isOwned && _sound.IapId != IapConstants.MsStoreAmbiePlusId)
                {
                    // Handle case that user is subscribed to ambie plus,
                    // but they don't own the individual add-on for some old premium sounds.
                    // We should let them download the sound as long as they have ambie plus.
                    isOwned = await _iapService.IsOwnedAsync(IapConstants.MsStoreAmbiePlusId);
                }
            }
            else
            {
                // a non premium sound is treated as "owned"
                isOwned = true;
            }

            IsOwned = isOwned;
        }

        private Task DownloadAsync()
        {
            if (DownloadProgressValue == 0 && NotInstalled)
            {
                _telemetry.TrackEvent(TelemetryConstants.DownloadClicked, new Dictionary<string, string>
                {
                    { "id", _sound.Id ?? "" },
                    { "location", TelemetryLocation }
                });

                return _downloadManager.QueueAndDownloadAsync(_sound, _downloadProgress);
            }

            return Task.CompletedTask;
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

        /// <inheritdoc/>
        public void Dispose()
        {
            _iapService.ProductPurchased -= OnProductPurchased;
            _downloadProgress.ProgressChanged -= OnProgressChanged;
            _soundDataProvider.LocalSoundDeleted -= OnSoundDeleted;
        }
    }
}
