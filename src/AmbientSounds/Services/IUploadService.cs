using AmbientSounds.Models;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for uploading sounds to the catalogue.
    /// </summary>
    public interface IUploadService
    {
        /// <summary>
        /// Uploads the given sound to the catalogue.
        /// </summary>
        /// <param name="s">The sound to upload.</param>
        Task UploadAsync(Sound s);
    }
}