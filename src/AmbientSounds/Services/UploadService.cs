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
        private readonly string _mySoundsUrl;
        private readonly HttpClient _client;
        private readonly IAccountManager _accountManager;
        private readonly IFilePicker _filePicker;

        /// <inheritdoc/>
        public event EventHandler<Sound>? SoundUploaded;

        /// <inheritdoc/>
        public event EventHandler<string>? SoundDeleted;

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
            _mySoundsUrl = appSettings.MySoundsUrl;
            _accountManager = accountManager;
            _client = httpClient;
            _filePicker = filePicker;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string id)
        {
            var accesstoken = await _accountManager.GetCatalogueTokenAsync();
            if (id is null || 
                string.IsNullOrWhiteSpace(accesstoken))
            {
                return false;
            }

            try
            {
                var url = _mySoundsUrl + $"/{id}";

                using var msg = new HttpRequestMessage(HttpMethod.Delete, url);
                msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);
                var response = await _client.SendAsync(msg);
                if (response.IsSuccessStatusCode)
                {
                    SoundDeleted?.Invoke(this, id);
                }

                return response.IsSuccessStatusCode;
            }
            catch
            {
                // TODO log
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task UploadAsync(Sound s)
        {
            var accesstoken = await _accountManager.GetCatalogueTokenAsync();
            if (s is null || string.IsNullOrWhiteSpace(accesstoken))
            {
                return;
            }

            byte[]? bytes = await _filePicker.GetCachedBytesAsync(s.FilePath);
            if (bytes is null)
            {
                throw new Exception($"Get bytes failed: {s.FilePath}");
            }

            // Clear filepath because we don't want
            // to upload the local file path.
            s.FilePath = "";
            var soundFileContent = new ByteArrayContent(bytes);

            using (var msg = new HttpRequestMessage(HttpMethod.Post, _uploadUrl))
            {
                msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);
                var content = new MultipartFormDataContent();
                var serialized = JsonSerializer.Serialize(s);
                content.Add(new StringContent(serialized, Encoding.UTF8, "application/json"), "soundData");
                content.Add(soundFileContent, "soundFile", s.Name);
                msg.Content = content;

                await _client.SendAsync(msg);
                SoundUploaded?.Invoke(this, s);
            }
        }
    }
}
