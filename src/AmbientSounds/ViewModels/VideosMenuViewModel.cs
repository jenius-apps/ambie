using AmbientSounds.Services;
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

        public VideosMenuViewModel(IVideoService videoService)
        {
            Guard.IsNotNull(videoService, nameof(videoService));

            _videoService = videoService;
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
                Videos.Add(new VideoViewModel(v, downloadCommand, deleteCommand));
            }
        }

        private async Task DownloadAsync(VideoViewModel? videoVm)
        {
            if (videoVm is null)
            {
                return;
            }

            videoVm.Video.IsDownloaded = videoVm.IsDownloaded = true;
            await Task.Delay(1);
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
