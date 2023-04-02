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
    private readonly ICatalogueService _catalogueService;
    private readonly IFileDownloader _fileDownloader;
    private readonly ISoundService _soundService;
    private readonly IAssetsReader _assetsReader;

    public event EventHandler? DownloadsCompleted;

    public WindowsDownloadManager(
        IFileDownloader fileDownloader,
        ISoundService soundService,
        ICatalogueService catalogueService,
        IAssetsReader assetsReader)
    {
        Guard.IsNotNull(fileDownloader);
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(catalogueService);
        Guard.IsNotNull(assetsReader);

        _fileDownloader = fileDownloader;
        _soundService = soundService;
        _catalogueService = catalogueService;
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

        var sounds = await _catalogueService.GetSoundsAsync(onlineSoundIds.ToArray());
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
    public async Task QueueUpdateAsync(
        Sound s,
        IProgress<double> progress,
        string previousImagePath,
        string previousFilePath,
        bool updateDataOnly = false)
    {
        bool performFakeDownload = updateDataOnly || _assetsReader.IsPathFromPackage(s.FilePath);
        string localImagePath = previousImagePath;
        string destinationFilePath = previousFilePath;

        if (performFakeDownload)
        {
            await QueueFakeAsync(progress);
        }
        else
        {
            (localImagePath, destinationFilePath) = await QueueAsync(s, progress);
        }

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
        Sound result;

        if (s is Guide g)
        {
            result = new Guide()
            {
                MinutesLength = g.MinutesLength,
                SuggestedBackgroundSounds = g.SuggestedBackgroundSounds
            };
        }
        else
        {
            result = new Sound();
        }

        result.Type = s.Type;
        result.Id = s.Id;
        result.ImagePath = localImagePath;
        result.Name = s.Name;
        result.FilePath = destinationFilePath;
        result.Attribution = s.Attribution;
        result.FileExtension = s.FileExtension;
        result.ScreensaverImagePaths = s.ScreensaverImagePaths;
        result.IsPremium = s.IsPremium;
        result.IapIds = s.IapIds.ToArray();
        result.ColourHex = s.ColourHex;
        result.ImagePaths = s.ImagePaths;
        result.Localizations = s.Localizations;
        result.MetaDataVersion = s.MetaDataVersion;
        result.FileVersion = s.FileVersion;

        return result;
    }

    private async Task QueueFakeAsync(IProgress<double> progress)
    {
        // Handle "downloading" of a packaged sound
        // As you might remember, a packaged sound is already local,
        // so we fake the download UX below to provide a consistent experience
        // for the user. 
        progress.Report(25);
        await Task.Delay(300);
        progress.Report(50);
    }

    private async Task<(string, string)> QueueAsync(Sound s, IProgress<double> progress)
    {
        string downloadUrl = await _catalogueService.GetDownloadLinkAsync(s);
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
            localImagePath = s.ImagePath;
            destinationFilePath = s.FilePath;
            await QueueFakeAsync(progress);
        }
        else
        {
            (localImagePath, destinationFilePath) = await QueueAsync(s, progress);
        }

        var newSoundInfo = CopySound(s, localImagePath, destinationFilePath);
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