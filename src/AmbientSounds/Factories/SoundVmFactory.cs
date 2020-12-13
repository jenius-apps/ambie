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

        public SoundVmFactory(
            IDownloadManager downloadManager,
            IMediaPlayerService player,
            ISoundDataProvider soundDataProvider)
        {
            Guard.IsNotNull(downloadManager, nameof(downloadManager));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(player, nameof(player));

            _downloadManager = downloadManager;
            _soundDataProvider = soundDataProvider;
            _player = player;
        }

        /// <inheritdoc/>
        public OnlineSoundViewModel GetOnlineSoundVm(Sound s)
        {
            Guard.IsNotNull(s, nameof(s));
            return new OnlineSoundViewModel(s, _downloadManager, _soundDataProvider);
        }

        /// <inheritdoc/>
        public SoundViewModel GetSoundVm(Sound s, int index)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsGreaterThan(index, -1, nameof(index));
            return new SoundViewModel(s, _player, index, _soundDataProvider);
        }
    }
}
