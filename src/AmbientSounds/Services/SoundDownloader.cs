using Microsoft.Toolkit.Diagnostics;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Downloads and saves sounds.
    /// </summary>
    public class SoundDownloader : ISoundDownloader
    {
        private readonly HttpClient _client;
        private readonly IFileWriter _fileWriter;

        public SoundDownloader(
            HttpClient httpClient,
            IFileWriter fileWriter)
        {
            Guard.IsNotNull(fileWriter, nameof(fileWriter));
            Guard.IsNotNull(httpClient, nameof(httpClient));
            _fileWriter = fileWriter;
            _client = httpClient;
        }

        /// <inheritdoc/>
        public async Task<string?> DownloadAndSaveAsync(string url, string nameWithExt)
        {
            if (string.IsNullOrWhiteSpace(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            using var stream = await _client.GetStreamAsync(url);
            return await _fileWriter.WriteSoundAsync(stream, nameWithExt);
        }
    }
}
