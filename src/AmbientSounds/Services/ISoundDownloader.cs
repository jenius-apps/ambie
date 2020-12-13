using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for downloading and saving sounds.
    /// </summary>
    public interface ISoundDownloader
    {
        /// <summary>
        /// Downloads sound and saves it to
        /// a local directory.
        /// </summary>
        /// <param name="url">The sound's URL.</param>
        /// <param name="nameWithExt">The sound's name with extension.</param>
        /// <returns></returns>
        Task<string?> DownloadAndSaveAsync(string? url, string nameWithExt);
    }
}