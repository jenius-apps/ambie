using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Writes ambie data to local directory.
    /// </summary>
    public interface IFileWriter
    {
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
        /// Reads the contents of the specified local file and returns the value.
        /// Returns string.Empty if file not found or path is invalid.
        /// </summary>
        /// <param name="relativeLocalPath">The relative path for a local file to read.</param>
        /// <param name="ct">Cancellation token.</param>
        Task<string> ReadAsync(string relativeLocalPath, CancellationToken ct = default);

        /// <summary>
        /// Writes the given string content to file in local storage.
        /// </summary>
        Task WriteStringAsync(string content, string relativeLocalPath);
        
        /// <summary>
        /// Deletes the given file.
        /// </summary>
        /// <param name="absolutePathInLocalStorage">The absolute path for a file inside local storage.
        /// If the path is outside of local storage, deletion will likely fail due to file permission restrictions.</param>
        /// <returns>True if successful, false otherwise.</returns>
        Task<bool> DeleteFileAsync(string absolutePathInLocalStorage);
    }
}