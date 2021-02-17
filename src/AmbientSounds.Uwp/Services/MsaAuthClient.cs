using AmbientSounds.Constants;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.ApplicationSettings;

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Class for signing into a Microsoft account and retrieving
    /// an auth token.
    /// </summary>
    public class MsaAuthClient : IMsaAuthClient
    {
        private const string MicrosoftAccountProviderId = "https://login.microsoft.com";
        private const string ConsumerAuthority = "consumers";
		private const string Scope = "User.Read";
		private const string PictureFileName = "profile.png";
		private readonly IUserSettings _userSettings;
		private readonly IFileWriter _fileWriter;
		private readonly string _clientId;

		/// <inheritdoc/>
		public event EventHandler InteractiveSignInCompleted;

        public MsaAuthClient(
			IUserSettings userSettings,
			IAppSettings appSettings,
			IFileWriter fileWriter)
        {
			Guard.IsNotNull(userSettings, nameof(userSettings));
			Guard.IsNotNull(appSettings, nameof(appSettings));
			Guard.IsNotNull(fileWriter, nameof(fileWriter));

			_userSettings = userSettings;
			_clientId = appSettings.MsaClientId;
			_fileWriter = fileWriter;
        }

		/// <inheritdoc/>
		public async Task SignOutAsync()
        {
			var result = await SilentAuthAsync();
			ClearStoredInfo();

			if (result == null)
            {
				return;
            }

			await result.ResponseData[0].WebAccount.SignOutAsync(_clientId);
        }

		/// <inheritdoc/>
		public async Task<string> GetPictureAsync()
        {
			var result = await SilentAuthAsync();

			if (result != null && result.ResponseStatus == WebTokenRequestStatus.Success)
            {
                var u = result.ResponseData[0].WebAccount.WebAccountProvider.User;
                var streamReference = await u.GetPictureAsync(UserPictureSize.Size64x64);
                if (streamReference != null)
                {
                    IRandomAccessStream stream = await streamReference.OpenReadAsync();
					return await _fileWriter.WriteBitmapAsync(stream.AsStream(), PictureFileName);
                }
            }

			return "";
		}

		/// <inheritdoc/>
		public async Task<string> GetTokenSilentAsync()
		{
			var result = await SilentAuthAsync();

			if (result != null && result.ResponseStatus == WebTokenRequestStatus.Success)
            {
				return result.ResponseData[0].Token;
            }

			return null;
		}

		private async Task<WebTokenRequestResult> SilentAuthAsync()
        {
			// Ref: https://docs.microsoft.com/en-us/windows/uwp/security/web-account-manager#store-the-account-for-future-use

			string providerId = _userSettings.Get<string>(UserSettingsConstants.CurrentUserProviderId);
			string accountId = _userSettings.Get<string>(UserSettingsConstants.CurrentUserId);

			if (string.IsNullOrWhiteSpace(providerId) || string.IsNullOrWhiteSpace(accountId))
			{
				return null;
			}

			try
            {
				WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(providerId);
				WebAccount account = await WebAuthenticationCoreManager.FindAccountAsync(provider, accountId);
				WebTokenRequest request = new WebTokenRequest(provider, Scope, _clientId);
				WebTokenRequestResult result = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(request, account);
				return result;
			}
            catch
            {
				return null;
            }
		}

		/// <inheritdoc/>
		public void RequestInteractiveSignIn()
        {
			// Ref for this method and all the related private methods below
			// https://github.com/microsoft/Windows-universal-samples/blob/master/Samples/WebAccountManagement/cs/SingleMicrosoftAccountScenario.xaml.cs
			var pane = AccountsSettingsPane.GetForCurrentView();
			pane.AccountCommandsRequested -= OnAccountCommandsRequested;
			pane.AccountCommandsRequested += OnAccountCommandsRequested;

			AccountsSettingsPane.Show();
		}

		private async void OnAccountCommandsRequested(AccountsSettingsPane sender, AccountsSettingsPaneCommandsRequestedEventArgs args)
		{
			AccountsSettingsPaneEventDeferral deferral = args.GetDeferral();
			await AddWebAccountProvider(args);
			//AddLinksAndDescription(args);
			deferral.Complete();
		}

		private async Task AddWebAccountProvider(AccountsSettingsPaneCommandsRequestedEventArgs args)
		{
			WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(
				MicrosoftAccountProviderId,
				ConsumerAuthority);

			WebAccountProviderCommand providerCommand = new WebAccountProviderCommand(
				provider,
				WebAccountProviderCommandInvoked);

			args.WebAccountProviderCommands.Add(providerCommand);
		}

		private async void WebAccountProviderCommandInvoked(WebAccountProviderCommand command)
		{
			await RequestTokenAndSaveAccount(command.WebAccountProvider, Scope, _clientId);
		}

		private async Task RequestTokenAndSaveAccount(WebAccountProvider Provider, string Scope, string ClientID)
		{
			ClearStoredInfo();

			try
			{
				WebTokenRequest webTokenRequest = new WebTokenRequest(Provider, Scope, ClientID);
				WebTokenRequestResult webTokenRequestResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest);

				if (webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.Success)
				{
					var response = webTokenRequestResult.ResponseData[0];
					_userSettings.Set(UserSettingsConstants.CurrentUserId, response.WebAccount.Id);
					_userSettings.Set(UserSettingsConstants.CurrentUserProviderId, response.WebAccount.WebAccountProvider.Id);
				}
			}
			catch
			{
			}

			InteractiveSignInCompleted?.Invoke(this, EventArgs.Empty);
		}

		private void ClearStoredInfo()
        {
			_userSettings.Set(UserSettingsConstants.CurrentUserId, "");
			_userSettings.Set(UserSettingsConstants.CurrentUserProviderId, "");
		}
	}
}
