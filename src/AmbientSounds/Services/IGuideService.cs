using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public interface IGuideService
{
    event EventHandler<string>? GuideDownloaded;
    event EventHandler<string>? GuideStopped;
    event EventHandler<string>? GuideStarted;

    Task<IReadOnlyList<Guide>> GetOnlineGuidesAsync(string? culture = null);
    Task<IReadOnlyList<Guide>> GetOfflineGuidesAsync();
    Task DownloadAsync(Guide guide, Progress<double> progress);
    Task<bool> DeleteAsync(string guideId);
    Task<Guide?> GetOfflineGuideAsync(string guideId);
    Task PlayAsync(Guide guide);
    void Stop(string guideId);
    void Stop();
}