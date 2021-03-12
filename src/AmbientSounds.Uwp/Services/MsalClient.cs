using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    public class MsalClient : IMsaAuthClient
    {
		private const string PictureFileName = "profile.png";
        private const string RedirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient";
        private const string Authority = "https://login.microsoftonline.com/common";
        private readonly string _clientId;
        private readonly IPublicClientApplication _msalSdkClient;
        private readonly HttpClient _httpClient;
        private readonly IFileWriter _fileWriter;

        /// <inheritdoc/>
        public event EventHandler? InteractiveSignInCompleted;

        public MsalClient(
            IAppSettings appSettings,
            IFileWriter fileWriter,
            HttpClient httpClient)
        {
            Guard.IsNotNull(appSettings, nameof(appSettings));
            Guard.IsNotNull(fileWriter, nameof(fileWriter));
            Guard.IsNotNull(httpClient, nameof(httpClient));

            _clientId = appSettings.MsaClientId;
            _fileWriter = fileWriter;
            _httpClient = httpClient;
            _msalSdkClient = PublicClientApplicationBuilder
                .Create(_clientId)
                .WithAuthority(Authority)
                .WithRedirectUri(RedirectUri)
                .Build();
        }

		/// <inheritdoc/>
        public async Task<Person> GetPersonDataAsync()
        {
            var graphToken = await GetTokenSilentAsync(new string[] { "User.Read" });
            if (string.IsNullOrWhiteSpace(graphToken))
            {
                return new Person();
            }

            var person = new Person();
            using var profileMsg = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me");
            profileMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", graphToken);
            var profileResponseTask = _httpClient.SendAsync(profileMsg);

            using var photoMsg = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/beta/me/photo/$value");
            photoMsg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", graphToken);
            var photoResponseTask = _httpClient.SendAsync(photoMsg);

            var profileResponse = await profileResponseTask;
            var photoResponse = await photoResponseTask;

            if (profileResponse.IsSuccessStatusCode)
            {
                var content = await profileResponse.Content.ReadAsStringAsync();
                var data = JObject.Parse(content);

                if (data != null)
                {
                    person.Email = data["userPrincipalName"].ToString();
                    person.Firstname = data["givenName"].ToString();
                }
            }

            if (photoResponse.IsSuccessStatusCode)
            {
                using var stream = await photoResponse.Content.ReadAsStreamAsync();
                person.PicturePath = await _fileWriter.WriteBitmapAsync(stream, PictureFileName);
            }

            return person;
        }

        /// <inheritdoc/>
        public async Task<string?> GetTokenSilentAsync(string[] scopes)
        {
            try
            {
                var accounts = await _msalSdkClient.GetAccountsAsync();
                var firstAccount = accounts.FirstOrDefault();
                var authResult = await _msalSdkClient 
                    .AcquireTokenSilent(scopes, firstAccount)
                    .ExecuteAsync();
                return authResult.AccessToken;
            }
            catch (Exception e)
            {
                return "";
            }
        }

		/// <inheritdoc/>
        public async void  RequestInteractiveSignIn(
            string[] scopes,
            string[]? extraScopes = null)
        {
            try
            {
                var builder = _msalSdkClient.AcquireTokenInteractive(scopes);
                if (extraScopes != null)
                {
                    builder = builder.WithExtraScopesToConsent(extraScopes);
                }

                var authResult = await builder.ExecuteAsync();
                
                if (!string.IsNullOrWhiteSpace(authResult.AccessToken))
                {
                    InteractiveSignInCompleted?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception e)
            {

            }
        }

        /// <inheritdoc/>
        public async Task SignOutAsync()
        {
            var accounts = await _msalSdkClient.GetAccountsAsync();
            foreach (var a in accounts)
            {
                await _msalSdkClient.RemoveAsync(a);
            }
        }
    }
}
