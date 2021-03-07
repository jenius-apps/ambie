using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for uploading sounds to the catalogue.
    /// </summary>
    public class UploadService : IUploadService
    {
        private readonly string _uploadUrl;
        private readonly HttpClient _client;
        private readonly IAccountManager _accountManager;
        private readonly IFilePicker _filePicker;

        public UploadService(
            HttpClient httpClient,
            IAppSettings appSettings,
            IFilePicker filePicker,
            IAccountManager accountManager)
        {
            Guard.IsNotNull(appSettings, nameof(appSettings));
            Guard.IsNotNull(accountManager, nameof(accountManager));
            Guard.IsNotNull(httpClient, nameof(httpClient));
            Guard.IsNotNull(filePicker, nameof(filePicker));

            _uploadUrl = appSettings.UploadUrl;
            _accountManager = accountManager;
            _client = httpClient;
            _filePicker = filePicker;
        }

        /// <inheritdoc/>
        public async Task UploadAsync(Sound s)
        {
            var accesstoken = await _accountManager.GetTokenAsync();
            if (s == null || string.IsNullOrWhiteSpace(accesstoken))
            {
                return;
            }

            byte[] bytes = await _filePicker.GetCachedBytesAsync(s.FilePath);
            if (bytes == null)
            {
                throw new Exception($"Get bytes failed: {s.FilePath}");
            }

            var soundFileContent = new ByteArrayContent(bytes);

            using (var msg = new HttpRequestMessage(HttpMethod.Post, _uploadUrl))
            {
                msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);
                var content = new MultipartFormDataContent();
                content.Add(new StringContent(JsonSerializer.Serialize(s), Encoding.UTF8, "application/json"), "soundData");
                content.Add(soundFileContent, "soundFile", s.Name);
                msg.Content = content;

                await _client.SendAsync(msg);
            }
        }
    }
}
