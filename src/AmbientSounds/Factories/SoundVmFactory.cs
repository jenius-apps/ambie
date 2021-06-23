using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;
using System;

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
        private readonly ISoundMixService _soundMixService;
        private readonly IRenamer _renamer;
        private readonly IUploadService _uploadService;
        private readonly IServiceProvider _serviceProvider;

        public SoundVmFactory(
            IDownloadManager downloadManager,
            IMixMediaPlayerService player,
            ITelemetry telemetry,
            IPreviewService previewService,
            ISoundDataProvider soundDataProvider,
            ISoundMixService soundMixService,
            IUserSettings userSettings,
            IIapService iapService,
            IUploadService uploadService,
            IRenamer renamer,
            IServiceProvider serviceProvider)
        {
            Guard.IsNotNull(downloadManager, nameof(downloadManager));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(player, nameof(player));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(iapService, nameof(iapService));
            Guard.IsNotNull(previewService, nameof(previewService));
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(soundMixService, nameof(soundMixService));
            Guard.IsNotNull(renamer, nameof(renamer));
            Guard.IsNotNull(uploadService, nameof(uploadService));
            Guard.IsNotNull(serviceProvider, nameof(serviceProvider));

            _userSettings = userSettings;
            _downloadManager = downloadManager;
            _previewService = previewService;
            _soundMixService = soundMixService;
            _iapService = iapService;
            _soundDataProvider = soundDataProvider;
            _player = player;
            _renamer = renamer;
            _telemetry = telemetry;
            _uploadService = uploadService;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public OnlineSoundViewModel? GetOnlineSoundVm(Sound s)
        {
            if (s is null ||
                s.Id is null ||
                s.ImagePath is null ||
                s.FilePath is null)
            {
                return null;
            }

            var dialogService = _serviceProvider.GetService(typeof(IDialogService)) as IDialogService;
            Guard.IsNotNull(dialogService, nameof(dialogService));

            return new OnlineSoundViewModel(
                s,
                _downloadManager,
                _soundDataProvider,
                _telemetry,
                _previewService,
                _iapService,
                dialogService);
        }

        /// <inheritdoc/>
        public UploadedSoundViewModel GetUploadedSoundVm(Sound s)
        {
            Guard.IsNotNull(s, nameof(s));
            return new UploadedSoundViewModel(s, _uploadService);
        }

        /// <inheritdoc/>
        public SoundViewModel GetSoundVm(Sound s)
        {
            Guard.IsNotNull(s, nameof(s));
            var vm = new SoundViewModel(
                s,
                _player,
                _soundDataProvider,
                _soundMixService,
                _telemetry,
                _renamer,
                _serviceProvider.GetRequiredService<IDialogService>(),
                _serviceProvider.GetRequiredService<IIapService>(),
                _serviceProvider.GetRequiredService<IDownloadManager>());
            vm.Initialize();
            return vm;
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
