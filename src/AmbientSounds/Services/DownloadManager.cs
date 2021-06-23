using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for downloading sounds.
    /// </summary>
    public class DownloadManager : IDownloadManager
    {
        private readonly IFileDownloader _fileDownloader;
        private readonly ISoundDataProvider _soundDataProvider;
        private readonly IOnlineSoundDataProvider _onlineSoundDataProvider;
        private readonly IIapService _iapService;
        private readonly Queue<QueuedSound> _downloadQueue = new();
        private bool _downloading;

        /// <inheritdoc/>
        public event EventHandler? DownloadsCompleted;

        public DownloadManager(
            IFileDownloader soundDownloader,
            ISoundDataProvider soundDataProvider,
            IOnlineSoundDataProvider onlineSoundDataProvider,
            IIapService iapService)
        {
            Guard.IsNotNull(soundDownloader, nameof(soundDownloader));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(onlineSoundDataProvider, nameof(onlineSoundDataProvider));
            Guard.IsNotNull(iapService, nameof(iapService));

            _fileDownloader = soundDownloader;
            _soundDataProvider = soundDataProvider;
            _onlineSoundDataProvider = onlineSoundDataProvider;
            _iapService = iapService;
        }

        /// <inheritdoc/>
        public async Task QueueAndDownloadAsync(Sound s, IProgress<double> progress)
        {
            if (s is null ||
                progress is null ||
                _downloadQueue.Any(x => s.Equals(x.SoundData)))
            {
                return;
            }

            progress.Report(1);
            _downloadQueue.Enqueue(new QueuedSound(s, progress));
            
            if (_downloading)
            {
                return;
            }

            _downloading = true;

            while (_downloadQueue.Count > 0)
            {
                QueuedSound item = _downloadQueue.Dequeue();

                try
                {
                    Sound soundData = item.SoundData;

                    // Confirm the sound is purchased if it's premium.
                    if (soundData.IsPremium)
                    {
                        var isOwned = await _iapService.IsOwnedAsync(soundData.IapId);
                        if (!isOwned)
                        {
                            throw new Exception("User hasn't purchased access to the sound. " + soundData.IapId);
                        }
                    }

                    item.Progress.Report(33);

                    string downloadUrl = await _onlineSoundDataProvider.GetDownloadLinkAsync(soundData);
                    if (string.IsNullOrWhiteSpace(downloadUrl))
                    {
                        item.Progress.Report(-1);
                        continue;
                    }

                    Task<string> downloadPathTask = _fileDownloader.SoundDownloadAndSaveAsync(
                        downloadUrl,
                        soundData.Id + soundData.FileExtension);
                    string localImagePath = await _fileDownloader.ImageDownloadAndSaveAsync(
                        soundData.ImagePath,
                        soundData.Id ?? "");

                    string localSoundPath = await downloadPathTask;

                    if (string.IsNullOrWhiteSpace(localSoundPath) ||
                        string.IsNullOrWhiteSpace(localImagePath))
                    {
                        item.Progress.Report(-1);
                        continue;
                    }

                    item.Progress.Report(66);

                    // add new record to local provider
                    var newSoundInfo = new Sound
                    {
                        Id = soundData.Id ?? "",
                        ImagePath = localImagePath,
                        Name = soundData.Name,
                        FilePath = localSoundPath,
                        Attribution = soundData.Attribution,
                        FileExtension = soundData.FileExtension,
                        ScreensaverImagePaths = soundData.ScreensaverImagePaths,
                        IsPremium = soundData.IsPremium,
                        IapId = soundData.IapId,
                        ColourHex = soundData.ColourHex
                    };

                    await _soundDataProvider.AddLocalSoundAsync(newSoundInfo);

                    // use delay to smoothen UX
                    await Task.Delay(300);
                    item.Progress.Report(100);
                }
                catch (Exception e)
                {
                    // TODO log
                    item.Progress.Report(0);
                }
            }

            DownloadsCompleted?.Invoke(this, EventArgs.Empty);
            _downloading = false;
        }

        public bool IsDownloadActive(Sound s)
        {
            throw new NotImplementedException();
        }

        public IProgress<double>? GetProgress(Sound s)
        {
            throw new NotImplementedException();
        }
    }
}
