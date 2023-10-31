using AmbientSounds.Cache;
using AmbientSounds.Extensions;
using AmbientSounds.Models;
using AmbientSounds.Repositories;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class GuideService : IGuideService
{
    private readonly IGuideCache _guideCache;
    private readonly ISystemInfoProvider _systemInfoProvider;
    private readonly IOnlineGuideRepository _onlineGuideRepository;
    private readonly IDownloadManager _downloadManager;
    private readonly IFileDownloader _fileDownloader;
    private readonly IFileWriter _fileWriter;
    private readonly IMixMediaPlayerService _mixMediaPlayerService;

    public event EventHandler<string>? GuideDownloaded;

    public event EventHandler<string>? GuideDeleted;

    public event EventHandler<string>? GuideStopped;

    public event EventHandler<string>? GuideStarted;

    public GuideService(
        IGuideCache guideCache,
        ISystemInfoProvider systemInfoProvider,
        IDownloadManager downloadManager,
        IOnlineGuideRepository onlineGuideRepository,
        IFileDownloader fileDownloader,
        IFileWriter fileWriter,
        IMixMediaPlayerService mixMediaPlayerService)
    {
        _guideCache = guideCache;
        _systemInfoProvider = systemInfoProvider;
        _onlineGuideRepository = onlineGuideRepository;
        _downloadManager = downloadManager;
        _fileDownloader = fileDownloader;
        _fileWriter = fileWriter;
        _mixMediaPlayerService = mixMediaPlayerService;

        _mixMediaPlayerService.GuidePositionChanged += OnGuidePositionChanged;
    }

    public async Task PlayAsync(Guide guide)
    {
        bool noActiveSounds = _mixMediaPlayerService.GetSoundIds() is { Length: 0 };

        if (_mixMediaPlayerService.IsSoundPlaying(guide.Id))
        {
            if (noActiveSounds)
            {
                await _mixMediaPlayerService.AddRandomAsync();
            }
            else
            {
                _mixMediaPlayerService.Play();
            }

            return;
        }

        if (await GetOfflineGuideAsync(guide.Id) is Guide offlineGuide)
        {
            if (noActiveSounds)
            {
                await _mixMediaPlayerService.AddRandomAsync();
            }

            // Only an offline guide can be played because its sound file is saved locally
            await _mixMediaPlayerService.PlayGuideAsync(offlineGuide);
            GuideStarted?.Invoke(this, guide.Id);
        }
    }

    public void Stop(string guideId)
    {
        if (guideId is { Length: > 0 })
        {
            _mixMediaPlayerService.RemoveSound(guideId);
            GuideStopped?.Invoke(this, guideId);
        }
    }

    public void Stop()
    {
        if (_mixMediaPlayerService.CurrentGuideId is { Length: > 0 } guideId)
        {
            Stop(guideId);
        }
    }

    public Task<IReadOnlyList<Guide>> GetOfflineGuidesAsync()
    {
        return _guideCache.GetOfflineGuidesAsync();
    }

    public async Task<IReadOnlyList<Guide>> GetOnlineGuidesAsync(string? culture = null)
    {
        culture ??= _systemInfoProvider.GetCulture();

        if (culture.Contains("-"))
        {
            culture = culture.Split('-')[0];
        }

        return await _guideCache.GetOnlineGuidesAsync(culture);
    }

    public Task<Guide?> GetOfflineGuideAsync(string guideId)
    {
        return _guideCache.GetOfflineGuideAsync(guideId);
    }

    public async Task DownloadAsync(Guide guide, Progress<double> progress)
    {
        // Populate downoad URL
        if (guide.DownloadUrl is not { Length: > 0 })
        {
            guide.DownloadUrl = await _onlineGuideRepository.GetDownloadUrlAsync(guide.Id);
            if (guide.DownloadUrl is { Length: 0 })
            {
                // If download URL isn't available, cancel this operation.
                throw new TaskCanceledException();
            }
        }

        // Subscribe to the progress so we can fire an event that 
        // it completed downloading later.
        progress.ProgressChanged += OnProgressChanged;

        // Queue for download
        var destinationPath = await _downloadManager.QueueAndDownloadAsync(guide, progress);
        if (string.IsNullOrEmpty(destinationPath))
        {
            // If the queue process failed to return a valid path,
            // cancel this operation.
            progress.ProgressChanged -= OnProgressChanged;
            throw new TaskCanceledException();
        }

        // Don't forget to download the guide's image.
        var imagePathTask = _fileDownloader.ImageDownloadAndSaveAsync(guide.ImagePath, guide.Id);

        // We perform a deep copy here because we don't want to modify
        // the properties of the existing guide, since that object is cached.
        var newGuide = guide.DeepCopy();

        // Overwrite only certain properties here.
        newGuide.FilePath = destinationPath;
        newGuide.IsDownloaded = true;
        newGuide.ImagePath = await imagePathTask;

        // Save the new data offline.
        await _guideCache.AddOfflineAsync(newGuide);

        void OnProgressChanged(object sender, double e)
        {
            if (e >= 100)
            {
                GuideDownloaded?.Invoke(this, guide.Id);
                progress.ProgressChanged -= OnProgressChanged;
            }
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteAsync(string guideId)
    {
        Guide? guide = await _guideCache.GetOfflineGuideAsync(guideId);
        if (guide is null)
        {
            return false;
        }

        var success = await _guideCache.RemoveOfflineAsync(guide.Id);
        if (success)
        {
            _mixMediaPlayerService.RemoveSound(guide.Id);
            _ = _fileWriter.DeleteFileAsync(guide.ImagePath);
            success = await _fileWriter.DeleteFileAsync(guide.FilePath);
        }
        
        if (success)
        {
            GuideDeleted?.Invoke(this, guide.Id);
        }

        return success;
    }

    private void OnGuidePositionChanged(object sender, TimeSpan e)
    {
        if (_mixMediaPlayerService.CurrentGuideId is { Length: > 0 } guideId)
        {
            if (_mixMediaPlayerService.GuideDuration > TimeSpan.MinValue &&
                _mixMediaPlayerService.GuideDuration == e)
            {
                Stop(guideId);
            }
        }
    }
}
