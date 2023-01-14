using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services;

public interface IShareService
{
    Task<ShareDetail?> GetShareDetailAsync(IReadOnlyList<string> soundIds);

    Task<ShareDetail?> GetShareDetailAsync(string shareId);

    Task<IReadOnlyList<string>> GetSoundIdsAsync(string shareId);
}