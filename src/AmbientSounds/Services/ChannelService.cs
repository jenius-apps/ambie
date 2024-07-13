using AmbientSounds.Cache;
using AmbientSounds.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class ChannelService : IChannelService
{
    private readonly ISoundCache _soundCache;

    public ChannelService(ISoundCache soundCache)
    {
        _soundCache = soundCache;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetChannelsAsync()
    {
        // TODO use page service to fetch list of sound IDs
        List<Sound> results = [];

        var onlineSounds = await _soundCache.GetOnlineSoundsAsync();
        var preInstalledSounds = await _soundCache.GetPreinstalledSoundsAsync();
        results.AddRange(onlineSounds.Where(x => x.AssociatedVideoIds.Count > 0));
        results.AddRange(preInstalledSounds.Where(x => x.AssociatedVideoIds.Count > 0));
        return results;
    }
}
