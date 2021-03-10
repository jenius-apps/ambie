using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

# nullable enable

namespace AmbientSounds.Services.Uwp
{
    public class MsalClient : IMsaAuthClient
    {
        private const string RedirectUri = "https://login.microsoftonline.com/common/oauth2/nativeclient";
        private const string Authority = "https://login.microsoftonline.com/common";
        private readonly string _clientId;
        private readonly IPublicClientApplication _msalSdkClient;

        /// <inheritdoc/>
        public event EventHandler? InteractiveSignInCompleted;

        public MsalClient(
            IAppSettings appSettings)
        {
            Guard.IsNotNull(appSettings, nameof(appSettings));

            _clientId = appSettings.MsaClientId;
            _msalSdkClient = PublicClientApplicationBuilder
                .Create(_clientId)
                .WithAuthority(Authority)
                .WithRedirectUri(RedirectUri)
                .Build();
        }

		/// <inheritdoc/>
        public Task<Person> GetPersonDataAsync()
        {
            // TODO get from graph beta endpoint?
            return Task.FromResult(new Person());
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
