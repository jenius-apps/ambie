using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public interface ISoundCache
{
    int InstallSoundsCount { get; }
    Task AddLocalInstalledSoundAsync(Sound sound);
    Task<IReadOnlyList<Sound>> GetPreinstalledSoundsAsync();
    Task<IReadOnlyList<Sound>> GetInstalledSoundsAsync();
    Task RemoveLocalInstalledSoundAsync(string stringId);
    Task<T?> GetInstalledSoundAsync<T>(string stringId) where T : Sound;
    Task SaveCacheAsync();
    Task<IReadOnlyList<Sound>> GetOnlineSoundsAsync();
    Task<IReadOnlyList<T>> GetOnlineSoundsAsync<T>(IReadOnlyList<string> soundIds) where T : Sound;
    Task<IReadOnlyList<Sound>> GetInstalledSoundsAsync(IEnumerable<string> soundIds);
}