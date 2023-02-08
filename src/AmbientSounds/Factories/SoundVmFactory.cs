﻿using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using System;
using JeniusApps.Common.Tools;

namespace AmbientSounds.Factories
{
    /// <summary>
    /// Creates sound viewmodels.
    /// </summary>
    public class SoundVmFactory : ISoundVmFactory
    {
        private readonly IDownloadManager _downloadManager;
        private readonly IMixMediaPlayerService _player;
        private readonly ITelemetry _telemetry;
        private readonly IIapService _iapService;
        private readonly IPreviewService _previewService;
        private readonly IUserSettings _userSettings;
        private readonly ISoundMixService _soundMixService;
        private readonly IRenamer _renamer;
        private readonly IServiceProvider _serviceProvider;

        public SoundVmFactory(
            IDownloadManager downloadManager,
            IMixMediaPlayerService player,
            ITelemetry telemetry,
            IPreviewService previewService,
            ISoundMixService soundMixService,
            IUserSettings userSettings,
            IIapService iapService,
            IRenamer renamer,
            IServiceProvider serviceProvider)
        {
            Guard.IsNotNull(downloadManager, nameof(downloadManager));
            Guard.IsNotNull(player, nameof(player));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(iapService, nameof(iapService));
            Guard.IsNotNull(previewService, nameof(previewService));
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(soundMixService, nameof(soundMixService));
            Guard.IsNotNull(renamer, nameof(renamer));
            Guard.IsNotNull(serviceProvider, nameof(serviceProvider));

            _userSettings = userSettings;
            _downloadManager = downloadManager;
            _previewService = previewService;
            _soundMixService = soundMixService;
            _iapService = iapService;
            _player = player;
            _renamer = renamer;
            _telemetry = telemetry;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public OnlineSoundViewModel GetOnlineSoundVm(Sound s)
        {
            Guard.IsNotNull(s);

            return new OnlineSoundViewModel(
                s,
                _downloadManager,
                _serviceProvider.GetRequiredService<ISoundService>(),
                _telemetry,
                _previewService,
                _iapService,
                _serviceProvider.GetRequiredService<IDialogService>(),
                _serviceProvider.GetRequiredService<IAssetLocalizer>(),
                _serviceProvider.GetRequiredService<IMixMediaPlayerService>(),
                _serviceProvider.GetRequiredService<IUpdateService>(),
                _serviceProvider.GetRequiredService<ILocalizer>());
        }

        /// <inheritdoc/>
        public SoundViewModel GetSoundVm(Sound s)
        {
            Guard.IsNotNull(s, nameof(s));
            var vm = new SoundViewModel(
                s,
                _player,
                _serviceProvider.GetRequiredService<ISoundService>(),
                _soundMixService,
                _telemetry,
                _renamer,
                _serviceProvider.GetRequiredService<IDialogService>(),
                _serviceProvider.GetRequiredService<IIapService>(),
                _serviceProvider.GetRequiredService<IDownloadManager>(),
                _serviceProvider.GetRequiredService<IPresenceService>(),
                _serviceProvider.GetRequiredService<IDispatcherQueue>(),
                _serviceProvider.GetRequiredService<IOnlineSoundDataProvider>(),
                _serviceProvider.GetRequiredService<IAssetLocalizer>());
            vm.Initialize();
            return vm;
        }

        /// <inheritdoc/>
        public ActiveTrackViewModel GetActiveTrackVm(Sound s, IRelayCommand<Sound> removeCommand)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsNotNull(removeCommand, nameof(removeCommand));
            return new ActiveTrackViewModel(
                s,
                removeCommand,
                _player,
                _userSettings,
                _serviceProvider.GetRequiredService<IAssetLocalizer>());
        }
    }
}
