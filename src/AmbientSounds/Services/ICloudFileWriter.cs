using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for writing and reading string content
    /// to a cloud storage.
    /// </summary>
    public interface ICloudFileWriter
    {
        /// <summary>
        /// Reads string content from the given URI.
        /// </summary>
        /// <param name="uri">The URI to read content from.</param>
        /// <param name="accessToken">An access token for authentication.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <returns>The string content of the given URI.</returns>
        Task<string> ReadFileAsync(string uri, string accessToken, CancellationToken ct);

        /// <summary>
        /// Writes the string content to the given URI.
        /// </summary>
        /// <param name="uri">The URI to write content to.</param>
        /// <param name="content">The serialized content that will be written.</param>
        /// <param name="accessToken">An access token for authentication.</param>
        /// <param name="ct">The cancellation token.</param>
        /// <param name="contentType">Optional. Default application/json.</param>
        Task WriteFileAsync(string uri, string content, string accessToken, CancellationToken ct, string contentType = "application/json");
    }
}