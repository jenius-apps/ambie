using Microsoft.Toolkit.Diagnostics;
using MimeTypes;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Downloads and saves sounds.
    /// </summary>
    public class FileDownloader : IFileDownloader
    {
        private readonly HttpClient _client;
        private readonly IFileWriter _fileWriter;

        public FileDownloader(
            HttpClient httpClient,
            IFileWriter fileWriter)
        {
            Guard.IsNotNull(fileWriter, nameof(fileWriter));
            Guard.IsNotNull(httpClient, nameof(httpClient));
            _fileWriter = fileWriter;
            _client = httpClient;
        }

        /// <inheritdoc/>
        public async Task<string> SoundDownloadAndSaveAsync(string? url, string nameWithExt)
        {
            if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return "";
            }

            using var stream = await _client.GetStreamAsync(url);
            return await _fileWriter.WriteSoundAsync(stream, nameWithExt);
        }

        /// <inheritdoc/>
        public async Task<string> ImageDownloadAndSaveAsync(string? url, string name)
        {
            if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return "";
            }

            HttpResponseMessage response = await _client.GetAsync(url);

            var contentType = response.Content.Headers.ContentType.MediaType;
            var nameWithExt = name + MimeTypeMap.GetExtension(contentType);
            using var stream = await response.Content.ReadAsStreamAsync();
            return await _fileWriter.WriteImageAsync(stream, nameWithExt);
        }
    }
}
