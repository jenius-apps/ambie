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

        /// <inheritdoc/>
        public event EventHandler<bool>? SignInUpdated;

        public AccountManager(IMsaAuthClient authClient)
        {
            Guard.IsNotNull(authClient, nameof(authClient));

            _authClient = authClient;
            _authClient.InteractiveSignInCompleted += OnSignInCompleted;
        }

        /// <inheritdoc/>
        public Task<string?> GetTokenAsync()
        {
            return _authClient.GetTokenSilentAsync();
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
            var token = await _authClient.GetTokenSilentAsync();
            return !string.IsNullOrWhiteSpace(token);
        }

        /// <inheritdoc/>
        public void RequestSignIn()
        {
            _authClient.RequestInteractiveSignIn();
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
                // GetPictureAsync can fail if the user declines
                // giving permission to access user picture data.
                return new Person();
            }
        }

        private async void OnSignInCompleted(object sender, EventArgs e)
        {
            var isSignedIn = await IsSignedInAsync();
            SignInUpdated?.Invoke(this, isSignedIn);
        }
    }
}
