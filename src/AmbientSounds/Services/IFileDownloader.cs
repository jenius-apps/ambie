using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for downloading and saving sounds.
    /// </summary>
    public interface IFileDownloader
    {
        /// <summary>
        /// Downloads image and saves it to
        /// a local directory.
        /// </summary>
        /// <param name="url">The image's URL.</param>
        /// <param name="name">The image's name.</param>
        /// <returns>Local path to image.</returns>
        Task<string> ImageDownloadAndSaveAsync(string? url, string name);
    }
}