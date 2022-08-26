using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
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
        private const string VideosDirectory = "videos";
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
            return GetProgress(destinationPath);
        }

        /// <inheritdoc/>
        public IProgress<double>? GetProgress(string destinationFilePath)
        {
            return BackgroundDownloadService.Instance.GetProgress(destinationFilePath);
        }

        /// <inheritdoc/>
        public async Task QueueAndDownloadAsync(IList<string> onlineSoundIds)
        {
            if (onlineSoundIds is null || onlineSoundIds.Count == 0)
            {
                return;
            }

            var sounds = await _onlineSoundDataProvider.GetSoundsAsync(onlineSoundIds);
            if (sounds is null)
            {
                return;
            }

            foreach (var s in sounds)
            {
                _ = QueueAndDownloadAsync(s, new Progress<double>());
            }
        }

        /// <inheritdoc/>
        public async Task<string> QueueAndDownloadAsync(Video video, IProgress<double> progress)
        {
            if (string.IsNullOrEmpty(video.DownloadUrl))
            {
                return string.Empty;
            }

            StorageFile destinationFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                $"{VideosDirectory}\\{video.Id + video.Extension}",
                CreationCollisionOption.ReplaceExisting);

            BackgroundDownloadService.Instance.StartDownload(
                destinationFile,
                video.DownloadUrl,
                progress);

            return destinationFile.Path;
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
