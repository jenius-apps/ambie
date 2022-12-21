using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public interface ISoundCache
{
    Task AddLocalInstalledSoundAsync(Sound sound);
    Task<IReadOnlyList<Sound>> GetPreinstalledSoundsAsync();
    Task<IReadOnlyList<Sound>> GetInstalledSoundsAsync();
    Task RemoveLocalInstalledSoundAsync(string stringId);
    Task<Sound?> GetInstalledSoundAsync(string stringId);
    Task SaveCacheAsync();
}