using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for the central hub for dealing with accounts.
    /// </summary>
    public class AccountManager : IAccountManager
    {
        private readonly IMsaAuthClient _authClient;

        public AccountManager(IMsaAuthClient authClient)
        {
            Guard.IsNotNull(authClient, nameof(authClient));

            _authClient = authClient;
        }

        /// <inheritdoc/>
        public async Task<bool> IsSignedInAsync()
        {
            var token = await _authClient.GetTokenSilentAsync();
            return !string.IsNullOrWhiteSpace(token);
        }
    }
}
