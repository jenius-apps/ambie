using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.ViewModels
{
    public class VideoViewModel : ObservableObject
    {
        private bool _isDownloaded;

        public VideoViewModel(
            Video video,
            IAsyncRelayCommand<VideoViewModel> downloadCommand,
            IAsyncRelayCommand<VideoViewModel> deleteCommand)
        {
            Guard.IsNotNull(video, nameof(video));
            Guard.IsNotNull(downloadCommand, nameof(downloadCommand));
            Guard.IsNotNull(deleteCommand, nameof(deleteCommand));
            Video = video;
            DownloadCommand = downloadCommand;
            DeleteCommand = deleteCommand;

            IsDownloaded = video.IsDownloaded;
        }

        public IAsyncRelayCommand<VideoViewModel> DownloadCommand { get; }

        public IAsyncRelayCommand<VideoViewModel> DeleteCommand { get; }

        public Video Video { get; }

        public bool IsDownloaded
        {
            get => _isDownloaded;
            set => SetProperty(ref _isDownloaded, value);
        }
    }
}
