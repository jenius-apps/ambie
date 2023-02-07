using AmbientSounds.Models;
using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

#nullable enable

namespace AmbientSounds.Services.Uwp;

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
    private readonly ISoundService _soundService;
    private readonly IAssetsReader _assetsReader;

    public event EventHandler? DownloadsCompleted;

    public WindowsDownloadManager(
        IFileDownloader fileDownloader,
        ISoundService soundService,
        IOnlineSoundDataProvider onlineSoundDataProvider,
        IAssetsReader assetsReader)
    {
        Guard.IsNotNull(fileDownloader);
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(onlineSoundDataProvider);
        Guard.IsNotNull(assetsReader);

        _fileDownloader = fileDownloader;
        _soundService = soundService;
        _onlineSoundDataProvider = onlineSoundDataProvider;
        _assetsReader = assetsReader;
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
    public async Task QueueUpdateAsync(Sound s, IProgress<double> progress, bool updateDataOnly = false)
    {
        bool performFakeDownload = updateDataOnly || _assetsReader.IsPathFromPackage(s.FilePath);

        (string localImagePath, string destinationFilePath) = performFakeDownload
            ? await QueueFakeAsync(s, progress)
            : await QueueAsync(s, progress);

        var newSoundInfo = CopySound(s, localImagePath, destinationFilePath);
        await _soundService.UpdateSoundAsync(newSoundInfo);

        if (performFakeDownload)
        {
            // Report completion of fake download here
            // so downstream event handlers work correctly.
            // The handlers expect the download completion to happen after
            // the sound metadata was added.
            await Task.Delay(300);
            progress.Report(100);
        }

        DownloadsCompleted?.Invoke(this, EventArgs.Empty);
    }

    private Sound CopySound(Sound s, string localImagePath, string destinationFilePath)
    {
        return new Sound()
        {
            Id = s.Id,
            ImagePath = localImagePath,
            Name = s.Name,
            FilePath = destinationFilePath,
            Attribution = s.Attribution,
            FileExtension = s.FileExtension,
            ScreensaverImagePaths = s.ScreensaverImagePaths,
            IsPremium = s.IsPremium,
            IapIds = s.IapIds.ToArray(),
            ColourHex = s.ColourHex,
            ImagePaths = s.ImagePaths,
            Localizations = s.Localizations,
            MetaDataVersion = s.MetaDataVersion,
            FileVersion = s.FileVersion
        };
    }

    private async Task<(string, string)> QueueFakeAsync(Sound s, IProgress<double> progress)
    {
        // Handle "downloading" of a packaged sound
        // As you might remember, a packaged sound is already local,
        // so we fake the download UX below to provide a consistent experience
        // for the user. 
        progress.Report(25);
        await Task.Delay(300);
        progress.Report(50);
        return (s.ImagePath, s.FilePath);
    }

    private async Task<(string, string)> QueueAsync(Sound s, IProgress<double> progress)
    {
        string downloadUrl = await _onlineSoundDataProvider.GetDownloadLinkAsync(s);
        if (string.IsNullOrWhiteSpace(downloadUrl))
        {
            return (string.Empty, string.Empty);
        }
        progress.Report(1);
        string localImagePath = await _fileDownloader.ImageDownloadAndSaveAsync(
            s.ImagePath,
            s.Id);
        StorageFile destinationFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            $"{SoundsDirectory}\\{s.Id + s.FileExtension}",
            CreationCollisionOption.ReplaceExisting);
        string destinationFilePath = destinationFile.Path;
        BackgroundDownloadService.Instance.StartDownload(
            destinationFile,
            downloadUrl,
            progress);

        return (localImagePath, destinationFilePath);
    }

    /// <inheritdoc/>
    public async Task QueueAndDownloadAsync(Sound s, IProgress<double> progress)
    {
        string localImagePath;
        string destinationFilePath;
        bool performFakeDownload = _assetsReader.IsPathFromPackage(s.FilePath);

        if (performFakeDownload)
        {
            // Handle "downloading" of a packaged sound
            // As you might remember, a packaged sound is already local,
            // so we fake the download UX below to provide a consistent experience
            // for the user. 
            progress.Report(25);
            await Task.Delay(300);
            progress.Report(50);
            localImagePath = s.ImagePath;
            destinationFilePath = s.FilePath;
        }
        else
        {
            string downloadUrl = await _onlineSoundDataProvider.GetDownloadLinkAsync(s);
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                return;
            }
            progress.Report(1);
            localImagePath = await _fileDownloader.ImageDownloadAndSaveAsync(
                s.ImagePath,
                s.Id);
            StorageFile destinationFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                $"{SoundsDirectory}\\{s.Id + s.FileExtension}",
                CreationCollisionOption.ReplaceExisting);
            destinationFilePath = destinationFile.Path;
            BackgroundDownloadService.Instance.StartDownload(
                destinationFile,
                downloadUrl,
                progress);
        }

        var newSoundInfo = new Sound
        {
            Id = s.Id,
            ImagePath = localImagePath,
            Name = s.Name,
            FilePath = destinationFilePath,
            Attribution = s.Attribution,
            FileExtension = s.FileExtension,
            ScreensaverImagePaths = s.ScreensaverImagePaths,
            IsPremium = s.IsPremium,
            IapIds = s.IapIds.ToArray(),
            ColourHex = s.ColourHex,
            ImagePaths = s.ImagePaths,
            Localizations = s.Localizations
        };

        await _soundService.AddLocalSoundAsync(newSoundInfo);

        if (performFakeDownload)
        {
            // Report completion of fake download here
            // so downstream event handlers work correctly.
            // The handlers expect the download completion to happen after
            // the sound metadata was added.
            await Task.Delay(300);
            progress.Report(100);
        }
        DownloadsCompleted?.Invoke(this, EventArgs.Empty);
    }

    private string GetDestinationPath(string soundFileName)
    {
        return Path.Combine(ApplicationData.Current.LocalFolder.Path, SoundsDirectory, soundFileName);
    }
}