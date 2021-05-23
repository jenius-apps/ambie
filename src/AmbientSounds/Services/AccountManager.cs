using AmbientSounds.Constants;
using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for the central hub for dealing with accounts.
    /// </summary>
    public class AccountManager : IAccountManager
    {
        private readonly IMsaAuthClient _authClient;
        private readonly string[] _catalogueScope;

        /// <inheritdoc/>
        public event EventHandler<bool>? SignInUpdated;

        public AccountManager(
            IMsaAuthClient authClient,
            IAppSettings appSettings)
        {
            Guard.IsNotNull(authClient, nameof(authClient));
            Guard.IsNotNull(appSettings, nameof(appSettings));

            _authClient = authClient;
            _catalogueScope = new string[] { appSettings.CatalogueScope };

            _authClient.InteractiveSignInCompleted += OnSignInCompleted;
        }

        /// <inheritdoc/>
        public Task<string?> GetGraphTokenAsync()
        {
            return _authClient.GetTokenSilentAsync(MsalConstants.GraphScopes);
        }

        /// <inheritdoc/>
        public Task<string?> GetCatalogueTokenAsync()
        {
            return _authClient.GetTokenSilentAsync(_catalogueScope);
        }

        /// <inheritdoc/>
        public async Task SignOutAsync()
        {
            await _authClient.SignOutAsync();
            SignInUpdated?.Invoke(this, false);
        }

        /// <inheritdoc/>
        public async Task<bool> IsSignedInAsync()
        {
            var token = await GetGraphTokenAsync();
            return !string.IsNullOrWhiteSpace(token);
        }

        /// <inheritdoc/>
        public Task RequestSignIn()
        {
            // Cannot combine graph and catalogue scopes
            // because together they cause an "incompatible scopes"
            // error when signing in.
            return _authClient.RequestInteractiveSignIn(
                MsalConstants.GraphScopes,
                _catalogueScope);
        }

        /// <inheritdoc/>
        public async Task<Person> GetPersonDataAsync()
        {
            try
            {
                return await _authClient.GetPersonDataAsync();
            }
            catch
            {
                // GetPersonDataAsync can fail if the user declines
                // giving permission to access user picture data.
                return new Person();
            }
        }

        private async void OnSignInCompleted(object sender, EventArgs e)
        {
            var isSignedIn = await IsSignedInAsync();
            SignInUpdated?.Invoke(this, isSignedIn);
        }

        public void Dispose()
        {
            _authClient.InteractiveSignInCompleted -= OnSignInCompleted;
        }
    }
}
