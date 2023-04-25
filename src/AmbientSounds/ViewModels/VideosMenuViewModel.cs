using AmbientSounds.Constants;
using AmbientSounds.Services;
using JeniusApps.Common.Tools;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using JeniusApps.Common.Telemetry;

namespace AmbientSounds.ViewModels
{
    public partial class VideosMenuViewModel : ObservableObject
    {
        private readonly IVideoService _videoService;
        private readonly ITelemetry _telemetry;
        private readonly ILocalizer _localizer;
        private readonly IIapService _iapService;

        [ObservableProperty]
        private bool _loading;

        public VideosMenuViewModel(
            IVideoService videoService,
            ITelemetry telemetry,
            ILocalizer localizer,
            IIapService iapService)
        {
            Guard.IsNotNull(videoService, nameof(videoService));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(localizer, nameof(localizer));
            Guard.IsNotNull(iapService, nameof(iapService));

            _videoService = videoService;
            _telemetry = telemetry;
            _localizer = localizer;
            _iapService = iapService;
            _iapService.ProductPurchased += OnIapPurchased;
        }

        public ObservableCollection<VideoViewModel> Videos { get; } = new();

        public async Task InitializeAsync()
        {
            Loading = true;
            Videos.Clear();
            var videos = await _videoService.GetVideosAsync();
            if (videos is null || videos.Count == 0)
            {
                Loading = false;
                return;
            }

            var downloadCommand = new AsyncRelayCommand<VideoViewModel>(DownloadAsync);
            var deleteCommand = new AsyncRelayCommand<VideoViewModel>(DeleteAsync);

            foreach (var v in videos)
            {
                Progress<double>? progress = _videoService.GetInstallProgress(v);
                var vm = new VideoViewModel(v, downloadCommand, deleteCommand, _localizer, progress);
                _ = UpdateOwnershipAsync(vm);
                Videos.Add(vm);
            }

            Loading = false;
        }

        private void OnIapPurchased(object sender, string e)
        {
            foreach (var v in Videos)
            {
                if (v.Video.IapIds.Contains(e))
                {
                    v.IsOwned = true;
                }
            }
        }

        private async Task UpdateOwnershipAsync(VideoViewModel videoVm)
        {
            if (!videoVm.Video.IsPremium)
            {
                videoVm.IsOwned = true;
                return;
            }

            videoVm.Loading = true;
            videoVm.IsOwned = await _iapService.IsAnyOwnedAsync(videoVm.Video.IapIds);
            videoVm.Loading = false;
        }

        private async Task DownloadAsync(VideoViewModel? videoVm)
        {
            if (videoVm is null)
            {
                return;
            }

            videoVm.Loading = true;

            _telemetry.TrackEvent(TelemetryConstants.VideoDownloadClicked, new Dictionary<string, string>
            {
                { "id", videoVm.Video.Id },
                { "name", videoVm.Video.Name }
            });

            await _videoService.InstallVideoAsync(videoVm.Video, videoVm.DownloadProgress);
        }

        private async Task DeleteAsync(VideoViewModel? videoVm)
        {
            if (videoVm is null)
            {
                return;
            }

            _telemetry.TrackEvent(TelemetryConstants.VideoDeleteClicked, new Dictionary<string, string>
            {
                { "id", videoVm.Video.Id },
                { "name", videoVm.Video.Name }
            });

            await _videoService.UninstallVideoAsync(videoVm.Video);
            videoVm.IsDownloaded = false;
        }
    }
}
