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
        private readonly ISoundDownloader _soundDownloader;
        private readonly ISoundDataProvider _soundDataProvider;
        private readonly Queue<QueuedSound> _downloadQueue = new();
        private bool _downloading;

        public DownloadManager(
            ISoundDownloader soundDownloader,
            ISoundDataProvider soundDataProvider)
        {
            Guard.IsNotNull(soundDownloader, nameof(soundDownloader));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));

            _soundDownloader = soundDownloader;
            _soundDataProvider = soundDataProvider;
        }

        /// <inheritdoc/>
        public async Task QueueAndDownloadAsync(Sound s, IProgress<double> progress)
        {
            if (s == null ||
                progress == null ||
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

            while (_downloadQueue.Count > 0)
            {
                try
                {
                    _downloading = true;
                    var item = _downloadQueue.Dequeue();
                    var soundData = item.SoundData;
                    item.Progress.Report(33);
                    string downloadPath = "";

                    downloadPath = await _soundDownloader.DownloadAndSaveAsync(
                        soundData.FilePath,
                        soundData.Id + soundData.FileExtension) ?? "";

                    if (string.IsNullOrWhiteSpace(downloadPath))
                    {
                        item.Progress.Report(-1);
                        continue;
                    }

                    item.Progress.Report(66);

                    // add new record to local provider
                    var newSoundInfo = new Sound
                    {
                        Id = soundData.Id,
                        ImagePath = soundData.ImagePath,
                        Name = soundData.Name,
                        FilePath = downloadPath,
                        Attribution = soundData.Attribution,
                        FileExtension = soundData.FileExtension
                    };

                    await _soundDataProvider.AddLocalSoundAsync(newSoundInfo);

                    // use delay to smoothen UX
                    await Task.Delay(300);
                    item.Progress.Report(100);
                }
                catch (Exception e)
                {
                    // TODO log
                }
            }

            _downloading = false;
        }
    }

    public record QueuedSound(Sound SoundData, IProgress<double> Progress);
}
