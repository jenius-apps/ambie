using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class OnlineSoundViewModel : ObservableObject
    {
        private readonly Sound _sound;
        private readonly IDownloadManager _downloadManager;
        private readonly Progress<double> _downloadProgress;
        private double _progressValue;

        public OnlineSoundViewModel(Sound s, IDownloadManager downloadManager)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsNotNull(downloadManager, nameof(downloadManager));
            _sound = s;
            _downloadManager = downloadManager;

            _downloadProgress = new Progress<double>();
            _downloadProgress.ProgressChanged += OnProgressChanged;
            DownloadCommand = new AsyncRelayCommand(DownloadAsync);
        }

        private void OnProgressChanged(object sender, double e)
        {
            DownloadProgressValue = e;
        }

        /// <summary>
        /// The sound's Id.
        /// </summary>
        public string Id => _sound.Id;

        /// <summary>
        /// The sound's attribution.
        /// </summary>
        public string Attribution => _sound.Attribution;

        /// <summary>
        /// Name of the sound.
        /// </summary>
        public string Name => _sound.Name ?? _sound.Id;

        /// <summary>
        /// The path for the image to display for the current sound.
        /// </summary>
        public string ImagePath => _sound.ImagePath;

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
            }
        }

        /// <summary>
        /// True if download progress should be visible.
        /// </summary>
        public bool DownloadProgressVisible => DownloadProgressValue > 0 && DownloadProgressValue < 100;

        /// <summary>
        /// Command for downloading this sound.
        /// </summary>
        public IAsyncRelayCommand DownloadCommand { get; }

        private Task DownloadAsync()
        {
            return _downloadManager.QueueAndDownloadAsync(_sound, _downloadProgress);
        }
    }
}
