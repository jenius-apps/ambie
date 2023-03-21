using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public interface IGuideService
{
    /// <summary>
    /// Retrieves guides based on the given list of IDs.
    /// </summary>
    /// <param name="guideIds">List of guide IDs to retrieve.</param>
    Task<IReadOnlyList<Guide>> GetGuidesAsync(IReadOnlyList<string> guideIds);
}