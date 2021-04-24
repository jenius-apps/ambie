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
        /// <returns>The path of the saved sound.</returns>
        Task<string> WriteSoundAsync(Stream stream, string nameWithExt);

        /// <summary>
        /// Writes sound image to local directory.
        /// </summary>
        /// <param name="stream">The stream of the image.</param>
        /// <param name="nameWithExt">The name of the file with extension. E.g. Wind.jpg.</param>
        /// <returns>The path of the saved image.</returns>
        Task<string> WriteImageAsync(Stream stream, string nameWithExt);

        /// <summary>
        /// Generic method for writing a file to local directory.
        /// </summary>
        /// <param name="stream">The stream of the file.</param>
        /// <param name="nameWithExt">The name of the file with extension. E.g. Wind.jpg.</param>
        /// <param name="localDirName">Optional. The name of the subdirectory to place the file in. 
        /// If null or empty, then the root local directory will be used.</param>
        /// <returns></returns>
        Task<string> WriteFileAsync(Stream stream, string nameWithExt, string? localDirName = null);

        /// <summary>
        /// Encodes and writes a bitmap image to the local directory.
        /// </summary>
        /// <param name="stream">The stream of the bitmap image to be encoded and written to file.</param>
        /// <param name="nameWithExt">The name of the file with extension. E.g. picture.png.</param>
        /// <returns>Path to image.</returns>
        Task<string> WriteBitmapAsync(Stream stream, string nameWithExt);
    }
}