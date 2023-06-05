using AmbientSounds.Cache;
using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class GuideService : IGuideService
{
    private readonly IGuideCache _guideCache;
    private readonly ISystemInfoProvider _systemInfoProvider;

    public GuideService(
        IGuideCache guideCache,
        ISystemInfoProvider systemInfoProvider)
    {
        _guideCache = guideCache;
        _systemInfoProvider = systemInfoProvider;
    }

    public async Task<IReadOnlyList<Guide>> GetGuidesAsync(string? culture = null)
    {
        culture ??= _systemInfoProvider.GetCulture();
        return await _guideCache.GetGuidesAsync(culture);
    }
}
