using AmbientSounds.Cache;
using AmbientSounds.Models;
using AmbientSounds.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class CatalogueService : ICatalogueService
{
    private readonly ISoundCache _soundCache;
    private readonly IOnlineSoundRepository _onlineSoundRepo;

    public CatalogueService(
        ISoundCache soundCache,
        IOnlineSoundRepository onlineSoundRepository)
    {
        _soundCache = soundCache;
        _onlineSoundRepo = onlineSoundRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetSoundsAsync()
    {
        var onlineSoundsTask = _soundCache.GetOnlineSoundsAsync();
        var preinstalled = await _soundCache.GetPreinstalledSoundsAsync();
        var result = new List<Sound>(preinstalled.OrderBy(x => x.Id));
        result.AddRange(await onlineSoundsTask);
        return result;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetSoundsAsync(IReadOnlyList<string> soundIds)
    {
        var onlineSoundsTask = _soundCache.GetOnlineSoundsAsync(soundIds);
        var preinstalled = await _soundCache.GetPreinstalledSoundsAsync();
        var result = new List<Sound>(preinstalled.Where(x => soundIds.Contains(x.Id)));
        result.AddRange(await onlineSoundsTask);
        return result;
    }

    /// <inheritdoc/>
    public Task<string> GetDownloadLinkAsync(Sound s)
    {
        return _onlineSoundRepo.GetDownloadLinkAsync(s);
    }
}
