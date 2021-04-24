using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for picking files.
    /// </summary>
    public interface IFilePicker
    {
        /// <summary>
        /// Opens the file picker and returns the path to the
        /// user selected item.
        /// </summary>
        /// <returns>Path to the selected file. Or returns empty string is cancelled.</returns>
        Task<string> OpenPickerAsync();

        /// <summary>
        /// Opens the file picker and returns the path to the
        /// user selected item.
        /// </summary>
        /// <returns>Path to the selected file. Or returns empty string is cancelled. And includes size in bytes.</returns>
        Task<(string, ulong)> OpenPickerAndGetSizeAsync();

        /// <summary>
        /// Returns the bytes of the cached given file path.
        /// Returns null of the file path was not found in cache.
        /// </summary>
        /// <remarks>
        /// This is a workaround for the file
        /// permission limitations in UWP.
        /// </remarks>
        /// <param name="filePath">Path to file whose bytes to retrieve.</param>
        /// <returns>Bytes of cached file, or null if file path was not found in cache.</returns>
        Task<byte[]?> GetCachedBytesAsync(string filePath);
    }
}
