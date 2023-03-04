using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface ICatalogueService
    {
        /// <summary>
        /// Retrieves download link for given sound.
        /// </summary>
        Task<string> GetDownloadLinkAsync(Sound s);

        /// <summary>
        /// Gets list of sounds that can be viewed
        /// on the catalogue.
        /// </summary>
        Task<IReadOnlyList<Sound>> GetSoundsAsync();
        Task<IReadOnlyList<Sound>> GetSoundsAsync(IReadOnlyList<string> soundIds);
    }
}