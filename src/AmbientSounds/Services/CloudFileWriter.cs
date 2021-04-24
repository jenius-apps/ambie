using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for reading and writing file string
    /// content to a cloud storage.
    /// </summary>
    public class CloudFileWriter : ICloudFileWriter
    {
        private readonly HttpClient _httpClient;

        public CloudFileWriter()
        {
            _httpClient = new HttpClient();
        }

        /// <inheritdoc/>
        public async Task<string> ReadFileAsync(
            string uri,
            string accessToken,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException(nameof(accessToken));
            }

            using (var msg = new HttpRequestMessage(HttpMethod.Get, uri))
            {
                msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                HttpResponseMessage httpResponse = await _httpClient.SendAsync(msg, ct);

                if (httpResponse.IsSuccessStatusCode)
                {
                    string responseString = await httpResponse.Content.ReadAsStringAsync();
                    return responseString;
                }
                else
                {
                    throw new Exception($"{httpResponse.StatusCode} {httpResponse.ReasonPhrase}");
                }
            }
        }

        /// <inheritdoc/>
        public async Task WriteFileAsync(
            string uri,
            string content,
            string accessToken,
            CancellationToken ct,
            string contentType = "application/json")
        {
            if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(accessToken))
            {
                throw new ArgumentNullException($"{nameof(content)} {nameof(accessToken)}");
            }

            using (var msg = new HttpRequestMessage(HttpMethod.Put, uri))
            {
                msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                msg.Content = new StringContent(content, Encoding.UTF8, contentType);
                HttpResponseMessage httpResponse = await _httpClient.SendAsync(msg, ct);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"{httpResponse.StatusCode} {httpResponse.ReasonPhrase}");
                }
            }
        }
    }
}
