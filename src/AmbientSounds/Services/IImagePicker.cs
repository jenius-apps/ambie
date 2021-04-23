using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for opening a file picker to select an image file.
    /// </summary>
    public interface IImagePicker
    {
        /// <summary>
        /// Opens a file picker to select an image.
        /// </summary>
        /// <returns>Returns path to file or null if no file selected.</returns>
        /// <remarks>
        /// The path returned may not be the original file path. It might
        /// be the path to a cached location of the file.
        /// </remarks>
        Task<string?> BrowseAsync();
    }
}
