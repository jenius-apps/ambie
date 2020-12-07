using System.IO;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Writes ambie data to local directory.
    /// </summary>
    public interface IFileWriter
    {
        /// <summary>
        /// Writes sound to local directory.
        /// </summary>
        /// <param name="stream">The stream of the sound to write.</param>
        /// <param name="nameWithExt">The name of the file with extension. E.g. Wind.mp3.</param>
        /// <returns>The path of the saved file.</returns>
        Task<string?> WriteSoundAsync(Stream stream, string nameWithExt);
    }
}