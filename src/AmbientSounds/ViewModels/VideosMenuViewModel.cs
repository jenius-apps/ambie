﻿using AmbientSounds.Constants;
using AmbientSounds.Services;
using JeniusApps.Common.Tools;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class VideosMenuViewModel : ObservableObject
    {
        private readonly IVideoService _videoService;
        private readonly ITelemetry _telemetry;
        private readonly IDownloadManager _downloadManager;
        private readonly ILocalizer _localizer;

        public VideosMenuViewModel(
            IVideoService videoService,
            ITelemetry telemetry,
            IDownloadManager downloadManager,
            ILocalizer localizer)
        {
            Guard.IsNotNull(videoService, nameof(videoService));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(downloadManager, nameof(downloadManager));
            Guard.IsNotNull(localizer, nameof(localizer));

            _videoService = videoService;
            _telemetry = telemetry;
            _downloadManager = downloadManager;
            _localizer = localizer;
        }

        public ObservableCollection<VideoViewModel> Videos { get; } = new();

        public async Task InitializeAsync()
        {
            if (Videos.Count > 0)
            {
                return;
            }

            var videos = await _videoService.GetVideosAsync();
            if (videos is null || videos.Count == 0)
            {
                return;
            }

            var downloadCommand = new AsyncRelayCommand<VideoViewModel>(DownloadAsync);
            var deleteCommand = new AsyncRelayCommand<VideoViewModel>(DeleteAsync);

            foreach (var v in videos)
            {
                Videos.Add(new VideoViewModel(v, downloadCommand, deleteCommand, _localizer));
            }
        }

        private async Task DownloadAsync(VideoViewModel? videoVm)
        {
            if (videoVm is null)
            {
                return;
            }

            if (string.IsNullOrEmpty(videoVm.Video.DownloadUrl))
            {
                videoVm.Video.DownloadUrl = await _videoService.GetDownloadUrlAsync(videoVm.Video.Id);
            }

            _telemetry.TrackEvent(TelemetryConstants.VideoDownloadClicked, new Dictionary<string, string>
            {
                { "id", videoVm.Video.Id },
                { "name", videoVm.Video.Name }
            });

            _ = _downloadManager.QueueAndDownloadAsync(videoVm.Video, videoVm.DownloadProgress);
        }

        private async Task DeleteAsync(VideoViewModel? videoVm)
        {
            if (videoVm is null)
            {
                return;
            }

            videoVm.Video.IsDownloaded = videoVm.IsDownloaded = false;
            await Task.Delay(1);
        }
    }
}