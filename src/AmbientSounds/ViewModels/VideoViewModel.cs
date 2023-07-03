using AmbientSounds.Models;
using JeniusApps.Common.Tools;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using AmbientSounds.Constants;

namespace AmbientSounds.ViewModels
{
    public partial class VideoViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isDownloaded;

        [ObservableProperty]
        private bool _isOwned;

        /// <summary>
        /// This sound's download progress.
        /// </summary>
        [ObservableProperty]
        private double _downloadProgressValue;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(DownloadProgressVisible))]
        private bool _loading;

        [ObservableProperty]
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
            Name = $"{video.Name} {FocusConstants.DotSeparator} {localizer.GetString("SizeMegaByte", video.MegaByteSize.ToString())}";
        }

        public string Name { get; }

        public Progress<double> DownloadProgress { get; }

        public IAsyncRelayCommand<VideoViewModel> DownloadCommand { get; }

        public IAsyncRelayCommand<VideoViewModel> DeleteCommand { get; }

        public Video Video { get; }

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
