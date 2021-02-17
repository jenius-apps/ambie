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

        public event EventHandler<bool>? SignInUpdated;

        public AccountManager(IMsaAuthClient authClient)
        {
            Guard.IsNotNull(authClient, nameof(authClient));

            _authClient = authClient;
            _authClient.InteractiveSignInCompleted += OnSignInCompleted;
        }

        private async void OnSignInCompleted(object sender, EventArgs e)
        {
            var isSignedIn = await IsSignedInAsync();
            SignInUpdated?.Invoke(this, isSignedIn);
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
        public async Task<string> GetPictureAsync()
        {
            try
            {
                return await _authClient.GetPictureAsync();
            }
            catch
            {
                // GetPictureAsync can fail if the user declines
                // giving permission to access user picture data.
                return "";
            }
        }
    }
}
