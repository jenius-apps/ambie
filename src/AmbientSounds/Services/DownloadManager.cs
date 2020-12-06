using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for downloading sounds.
    /// </summary>
    public class DownloadManager : IDownloadManager
    {
        private readonly Queue<QueuedSound> _downloadQueue = new();
        private bool _downloading;

        public DownloadManager()
        {
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
                item.Progress.Report(0);

                // download item and get new record
                item.Progress.Report(33);

                // add new record to local provider
                await Task.Delay(3000);
                item.Progress.Report(66);
                await Task.Delay(3000);

                item.Progress.Report(100);

            }

            _downloading = false;
        }
    }

    public record QueuedSound(Sound SoundData, IProgress<double> Progress);
}
