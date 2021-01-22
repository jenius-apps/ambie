using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Toolkit.Diagnostics;

namespace AmbientSounds.Factories
{
    /// <summary>
    /// Creates sound viewmodels.
    /// </summary>
    public class SoundVmFactory : ISoundVmFactory
    {
        private readonly IDownloadManager _downloadManager;
        private readonly ISoundDataProvider _soundDataProvider;
        private readonly IMediaPlayerService _player;
        private readonly ITelemetry _telemetry;
        private readonly IIapService _iapService;

        public SoundVmFactory(
            IDownloadManager downloadManager,
            IMediaPlayerService player,
            ITelemetry telemetry,
            ISoundDataProvider soundDataProvider,
            IIapService iapService)
        {
            Guard.IsNotNull(downloadManager, nameof(downloadManager));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(player, nameof(player));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(iapService, nameof(iapService));

            _downloadManager = downloadManager;
            _iapService = iapService;
            _soundDataProvider = soundDataProvider;
            _player = player;
            _telemetry = telemetry;
        }

        /// <inheritdoc/>
        public OnlineSoundViewModel? GetOnlineSoundVm(Sound s)
        {
            if (s == null ||
                s.Id == null ||
                s.ImagePath == null ||
                s.FilePath == null)
            {
                return null;
            }

            return new OnlineSoundViewModel(
                s,
                _downloadManager,
                _soundDataProvider,
                _telemetry,
                _iapService);
        }

        /// <inheritdoc/>
        public SoundViewModel GetSoundVm(Sound s, int index)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsGreaterThan(index, -1, nameof(index));
            return new SoundViewModel(s, _player, index, _soundDataProvider, _telemetry);
        }
    }
}
