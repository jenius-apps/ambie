using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public interface IGuideService
{
    event EventHandler<string>? GuideDownloaded;

    Task<IReadOnlyList<Guide>> GetGuidesAsync(string? culture = null);

    Task DownloadAsync(Guide guide, Progress<double> progress);

    Guide? GetCachedGuide(string guideId);
}