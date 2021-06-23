using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Uses the Windows-provided BackgroundDownloader
    /// API to perform download and track progress.
    /// </summary>
    public class WindowsDownloadManager : IDownloadManager
    {
        private const string SoundsDirectory = "sounds";
        private readonly IOnlineSoundDataProvider _onlineSoundDataProvider;
        private readonly IFileDownloader _fileDownloader;
        private readonly ISoundDataProvider _soundDataProvider;

        public event EventHandler? DownloadsCompleted;

        public WindowsDownloadManager(
            IFileDownloader fileDownloader,
            ISoundDataProvider soundDataProvider,
            IOnlineSoundDataProvider onlineSoundDataProvider)
        {
            Guard.IsNotNull(fileDownloader, nameof(fileDownloader));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(onlineSoundDataProvider, nameof(onlineSoundDataProvider));

            _fileDownloader = fileDownloader;
            _soundDataProvider = soundDataProvider;
            _onlineSoundDataProvider = onlineSoundDataProvider;
        }

        /// <inheritdoc/>
        public bool IsDownloadActive(Sound s)
        {
            string destinationPath = GetDestinationPath(s.Id + s.FileExtension);
            return BackgroundDownloadService.Instance.IsDownloadActive(destinationPath);
        }

        /// <inheritdoc/>
        public IProgress<double>? GetProgress(Sound s)
        {
            string destinationPath = GetDestinationPath(s.Id + s.FileExtension);
            return BackgroundDownloadService.Instance.GetProgress(destinationPath);
        }

        /// <inheritdoc/>
        public async Task QueueAndDownloadAsync(Sound s, IProgress<double> progress)
        {
            string downloadUrl = await _onlineSoundDataProvider.GetDownloadLinkAsync(s);
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                return;
            }

            progress.Report(1);

            string localImagePath = await _fileDownloader.ImageDownloadAndSaveAsync(
                s.ImagePath,
                s.Id);

            StorageFile destinationFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                $"{SoundsDirectory}\\{s.Id + s.FileExtension}",
                CreationCollisionOption.ReplaceExisting);

            BackgroundDownloadService.Instance.StartDownload(
                destinationFile,
                downloadUrl,
                progress);

            var newSoundInfo = new Sound
            {
                Id = s.Id,
                ImagePath = localImagePath,
                Name = s.Name,
                FilePath = destinationFile.Path,
                Attribution = s.Attribution,
                FileExtension = s.FileExtension,
                ScreensaverImagePaths = s.ScreensaverImagePaths,
                IsPremium = s.IsPremium,
                IapId = s.IapId,
                ColourHex = s.ColourHex
            };

            await _soundDataProvider.AddLocalSoundAsync(newSoundInfo);
            DownloadsCompleted?.Invoke(this, EventArgs.Empty);
        }

        private string GetDestinationPath(string soundFileName)
        {
            return Path.Combine(ApplicationData.Current.LocalFolder.Path, SoundsDirectory, soundFileName);
        }
    }
}
