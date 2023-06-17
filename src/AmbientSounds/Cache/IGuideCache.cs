using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Cache
{
    public interface IGuideCache
    {
        /// <summary>
        /// Retrieves list of online guide data
        /// based on the given culture.
        /// </summary>
        /// <param name="culture">The culture of online guide data to retrieve.</param>
        /// <returns>List of online guide data.</returns>
        Task<IReadOnlyList<Guide>> GetOnlineGuidesAsync(string culture);

        /// <summary>
        /// Retrieves list of offline guide data. 
        /// This is data that is available offline.
        /// </summary>
        /// <returns>List of all offline guide data, regardless of culture.</returns>
        Task<IReadOnlyList<Guide>> GetOfflineGuidesAsync();

        /// <summary>
        /// Retrieves given offline guide data.
        /// </summary>
        /// <param name="guideId">ID of the guide to retrieve.</param>
        /// <returns>The requested offline guide if it exists. Returns null, otherwise.</returns>
        Task<Guide?> GetOfflineGuideAsync(string guideId);

        /// <summary>
        /// Adds the given guide object to offline storage.
        /// </summary>
        /// <param name="guide">The guide object to store offline.</param>
        Task AddOfflineAsync(Guide guide);

        /// <summary>
        /// Removes the give guide from offline storage.
        /// </summary>
        /// <param name="guideId">ID of the guide to remove.</param>
        /// <returns>True if successfully removed. False, otherwise.</returns>
        Task<bool> RemoveOfflineAsync(string guideId);
    }
}