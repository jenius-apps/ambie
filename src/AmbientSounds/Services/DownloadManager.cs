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
        private readonly Queue<QueuedSound> _downloadQueue = new();
        private bool _downloading;

        public DownloadManager(ISoundDownloader soundDownloader)
        {
            Guard.IsNotNull(soundDownloader, nameof(soundDownloader));

            _soundDownloader = soundDownloader;
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

            _downloadQueue.Enqueue(new QueuedSound(s, progress));
            
            if (_downloading)
            {
                return;
            }

            while (_downloadQueue.Count > 0)
            {
                _downloading = true;
                var item = _downloadQueue.Dequeue();
                var soundData = item.SoundData;
                item.Progress.Report(0);

                // download item and get new record
                item.Progress.Report(33);

                string downloadPath = "";

                try
                {
                    downloadPath = await _soundDownloader.DownloadAndSaveAsync(
                        soundData.FilePath,
                        soundData.Id + ".mp3") ?? "";
                }
                catch (Exception e)
                {
                    // TODO log
                }

                if (string.IsNullOrWhiteSpace(downloadPath))
                {
                    item.Progress.Report(-1);
                    continue;
                }

                // add new record to local provider
                await Task.Delay(3000);
                item.Progress.Report(66);
                await Task.Delay(3000);

                item.Progress.Report(100);

                var result = new Sound(soundData.Id, "", soundData.Name, downloadPath, soundData.Attribution);

            }

            _downloading = false;
        }
    }

    public record QueuedSound(Sound SoundData, IProgress<double> Progress);
}
