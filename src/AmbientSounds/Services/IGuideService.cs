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

    /// <summary>
    /// Using the suggested background sounds in the given guide object,
    /// this method will hydrate the necessary sound objects and it will
    /// return a list of sound mixes that are readily formatted.
    /// </summary>
    /// <param name="g">The guide to use.</param>
    /// <returns>List of sound mixes that match the suggested background sounds of the given guide.</returns>
    Task<IReadOnlyList<Sound>> GetSuggestedSoundMixesAsync(Guide g);
}