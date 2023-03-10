﻿using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using System;
using JeniusApps.Common.Tools;
using AmbientSounds.Repositories;
using System.Collections.Concurrent;

namespace AmbientSounds.Factories
{
    /// <summary>
    /// Creates sound viewmodels.
    /// </summary>
    public class SoundVmFactory : ISoundVmFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConcurrentDictionary<string, OnlineSoundViewModel> _onlineSoundVmCache = new();

        public SoundVmFactory(IServiceProvider serviceProvider)
        {
            Guard.IsNotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public OnlineSoundViewModel GetOnlineSoundVm(Sound s)
        {
            Guard.IsNotNull(s);

            if (_onlineSoundVmCache.TryGetValue(s.Id, out var vm))
            {
                return vm;
            }

            var newVm = new OnlineSoundViewModel(
                s,
                _serviceProvider.GetRequiredService<IDownloadManager>(),
                _serviceProvider.GetRequiredService<ISoundService>(),
                _serviceProvider.GetRequiredService<ITelemetry>(),
                _serviceProvider.GetRequiredService<IPreviewService>(),
                _serviceProvider.GetRequiredService<IIapService>(),
                _serviceProvider.GetRequiredService<IDialogService>(),
                _serviceProvider.GetRequiredService<IAssetLocalizer>(),
                _serviceProvider.GetRequiredService<IMixMediaPlayerService>(),
                _serviceProvider.GetRequiredService<IUpdateService>(),
                _serviceProvider.GetRequiredService<ILocalizer>());

            _onlineSoundVmCache.TryAdd(s.Id, newVm);
            return newVm;
        }

        /// <inheritdoc/>
        public SoundViewModel GetSoundVm(Sound s)
        {
            Guard.IsNotNull(s, nameof(s));
            var vm = new SoundViewModel(
                s,
                _serviceProvider.GetRequiredService<IMixMediaPlayerService>(),
                _serviceProvider.GetRequiredService<ISoundService>(),
                _serviceProvider.GetRequiredService<ISoundMixService>(),
                _serviceProvider.GetRequiredService<ITelemetry>(),
                _serviceProvider.GetRequiredService<IRenamer>(),
                _serviceProvider.GetRequiredService<IDialogService>(),
                _serviceProvider.GetRequiredService<IIapService>(),
                _serviceProvider.GetRequiredService<IDownloadManager>(),
                _serviceProvider.GetRequiredService<IPresenceService>(),
                _serviceProvider.GetRequiredService<IDispatcherQueue>(),
                _serviceProvider.GetRequiredService<IOnlineSoundRepository>(),
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
                _serviceProvider.GetRequiredService<IMixMediaPlayerService>(),
                _serviceProvider.GetRequiredService<IUserSettings>(),
                _serviceProvider.GetRequiredService<IAssetLocalizer>());
        }
    }
}
