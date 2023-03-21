using AmbientSounds.Cache;
using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public sealed class GuideService : IGuideService
{
    private readonly ISoundCache _soundcache;

    public GuideService(
        ISoundCache soundCache)
    {
        _soundcache = soundCache;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Guide>> GetGuidesAsync(IReadOnlyList<string> guideIds)
    {
        if (guideIds is { Count: 0 })
        {
            return Array.Empty<Guide>();
        }

        return await _soundcache.GetOnlineSoundsAsync<Guide>(guideIds);
    }
}
