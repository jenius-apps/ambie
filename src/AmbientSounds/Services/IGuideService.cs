using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public interface IGuideService
{
    Task<IReadOnlyList<Guide>> GetGuidesAsync(string? culture = null);
}