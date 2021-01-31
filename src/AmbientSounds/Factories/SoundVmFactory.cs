using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;

namespace AmbientSounds.Factories
{
    /// <summary>
    /// Creates sound viewmodels.
    /// </summary>
    public class SoundVmFactory : ISoundVmFactory
    {
        private readonly IDownloadManager _downloadManager;
        private readonly ISoundDataProvider _soundDataProvider;
        private readonly IMixMediaPlayerService _player;
        private readonly ITelemetry _telemetry;
        private readonly IIapService _iapService;
        private readonly IPreviewService _previewService;
        private readonly IUserSettings _userSettings;

        public SoundVmFactory(
            IDownloadManager downloadManager,
            IMixMediaPlayerService player,
            ITelemetry telemetry,
            IPreviewService previewService,
            ISoundDataProvider soundDataProvider,
            IUserSettings userSettings,
            IIapService iapService)
        {
            Guard.IsNotNull(downloadManager, nameof(downloadManager));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(player, nameof(player));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(iapService, nameof(iapService));
            Guard.IsNotNull(previewService, nameof(previewService));
            Guard.IsNotNull(userSettings, nameof(userSettings));

            _userSettings = userSettings;
            _downloadManager = downloadManager;
            _previewService = previewService;
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
                _previewService,
                _iapService);
        }

        /// <inheritdoc/>
        public SoundViewModel GetSoundVm(Sound s, int index)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsGreaterThan(index, -1, nameof(index));
            return new SoundViewModel(s, _player, index, _soundDataProvider, _telemetry);
        }

        /// <inheritdoc/>
        public ActiveTrackViewModel GetActiveTrackVm(Sound s, IRelayCommand<Sound> removeCommand)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsNotNull(removeCommand, nameof(removeCommand));
            return new ActiveTrackViewModel(s, removeCommand, _player, _userSettings);
        }
    }
}
