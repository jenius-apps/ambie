using AmbientSounds.Models;
using JeniusApps.Common.Tools;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace AmbientSounds.ViewModels
{
    public class VideoViewModel : ObservableObject
    {
        private bool _isDownloaded;
        private bool _isOwned;
        private double _progressValue;
        private bool _loading;
        private bool _downloadProgressVisible;

        public VideoViewModel(
            Video video,
            IAsyncRelayCommand<VideoViewModel> downloadCommand,
            IAsyncRelayCommand<VideoViewModel> deleteCommand,
            ILocalizer localizer,
            Progress<double>? downloadProgress = null)
        {
            Guard.IsNotNull(video, nameof(video));
            Guard.IsNotNull(downloadCommand, nameof(downloadCommand));
            Guard.IsNotNull(deleteCommand, nameof(deleteCommand));
            Video = video;
            DownloadCommand = downloadCommand;
            DeleteCommand = deleteCommand;

            DownloadProgress = downloadProgress ?? new Progress<double>();
            DownloadProgress.ProgressChanged += OnProgressChanged;
            
            IsDownloaded = video.IsDownloaded;
            Name = $"{video.Name} • {localizer.GetString("SizeMegaByte", video.MegaByteSize.ToString())}";
        }

        public string Name { get; }

        public Progress<double> DownloadProgress { get; }

        public IAsyncRelayCommand<VideoViewModel> DownloadCommand { get; }

        public IAsyncRelayCommand<VideoViewModel> DeleteCommand { get; }

        public Video Video { get; }

        /// <summary>
        /// This sound's download progress.
        /// </summary>
        public double DownloadProgressValue
        {
            get => _progressValue;
            set
            {
                SetProperty(ref _progressValue, value);
            }
        }

        public bool DownloadProgressVisible
        {
            get => _downloadProgressVisible;
            set => SetProperty(ref _downloadProgressVisible, value);
        }

        public bool IsDownloaded
        {
            get => _isDownloaded;
            set => SetProperty(ref _isDownloaded, value);
        }

        public bool IsOwned
        {
            get => _isOwned;
            set 
            {
                SetProperty(ref _isOwned, value);
            }
        }

        public bool Loading
        {
            get => _loading;
            set
            {
                SetProperty(ref _loading, value);
                OnPropertyChanged(nameof(DownloadProgressVisible));
            }
        }

        private void OnProgressChanged(object sender, double e)
        {
            if (e <= 0)
            {
                Loading = true;
                DownloadProgressVisible = false;
            }
            else if (e >= 1 && e < 100)
            {
                Loading = false;
                DownloadProgressVisible = true;
            }
            else if (e >= 100)
            {
                IsDownloaded = true;
                DownloadProgressVisible = false;
            }

            DownloadProgressValue = e;
        }
    }
}
