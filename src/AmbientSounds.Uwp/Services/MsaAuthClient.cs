using AmbientSounds.Constants;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;

namespace AmbientSounds.Services.Uwp
{
	/// <summary>
	/// Class for signing into a Microsoft account and retrieving
	/// an auth token.
	/// </summary>
    public class MsaAuthClient : IMsaAuthClient
    {
		private const string Scope = "User.Read";
		private readonly IUserSettings _userSettings;
		private readonly string _clientId;

        public MsaAuthClient(
			IUserSettings userSettings,
			IAppSettings appSettings)
        {
			Guard.IsNotNull(userSettings, nameof(userSettings));
			Guard.IsNotNull(appSettings, nameof(appSettings));

			_userSettings = userSettings;
			_clientId = appSettings.MsaClientId;
        }

		/// <inheritdoc/>
        public async Task<string> GetTokenSilentAsync()
        {
			// Ref: https://docs.microsoft.com/en-us/windows/uwp/security/web-account-manager#store-the-account-for-future-use

			string providerId = _userSettings.Get<string>(UserSettingsConstants.CurrentUserProviderId);
			string accountId = _userSettings.Get<string>(UserSettingsConstants.CurrentUserId);

			if (string.IsNullOrWhiteSpace(providerId) || string.IsNullOrWhiteSpace(accountId))
			{
				return null;
			}

			WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId);
			WebAccount account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountId);

			WebTokenRequest request = new WebTokenRequest(provider, Scope, _clientId);

			WebTokenRequestResult result = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request, account);
			if (result.ResponseStatus == WebTokenRequestStatus.UserInteractionRequired)
			{
				// Unable to get a token silently - you'll need to show the UI
				return null;
			}
			else if (result.ResponseStatus == WebTokenRequestStatus.Success)
			{
				// Success
				return result.ResponseData[0].Token;
			}
			else
			{
				// Other error 
				return null;
			}
		}
    }
}
