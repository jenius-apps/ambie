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

        public SoundVmFactory(
            IDownloadManager downloadManager,
            ISoundDataProvider soundDataProvider)
        {
            Guard.IsNotNull(downloadManager, nameof(downloadManager));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));

            _downloadManager = downloadManager;
            _soundDataProvider = soundDataProvider;
        }

        /// <inheritdoc/>
        public OnlineSoundViewModel GetOnlineSoundVm(Sound s)
        {
            Guard.IsNotNull(s, nameof(s));
            return new OnlineSoundViewModel(s, _downloadManager, _soundDataProvider);
        }
    }
}
