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
        private readonly Queue<QueuedSound> _downloadQueue = new();
        private bool _downloading;

        public DownloadManager(
            IFileDownloader soundDownloader,
            ISoundDataProvider soundDataProvider)
        {
            Guard.IsNotNull(soundDownloader, nameof(soundDownloader));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));

            _fileDownloader = soundDownloader;
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

            _downloading = true;

            while (_downloadQueue.Count > 0)
            {
                var item = _downloadQueue.Dequeue();

                try
                {
                    var soundData = item.SoundData;
                    item.Progress.Report(33);

                    Task<string> downloadPathTask = _fileDownloader.SoundDownloadAndSaveAsync(
                        soundData.FilePath,
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
                        ScreensaverImagePaths = soundData.ScreensaverImagePaths
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

            _downloading = false;
        }
    }

    public record QueuedSound(Sound SoundData, IProgress<double> Progress);
}
