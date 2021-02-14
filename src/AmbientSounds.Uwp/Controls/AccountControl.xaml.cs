using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AmbientSounds.Controls
{
    public sealed partial class AccountControl : UserControl
    {
        public AccountControl()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AccountsSettingsPane.Show();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += OnAccountCommandsRequested;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested -= OnAccountCommandsRequested;
        }

        private async void OnAccountCommandsRequested(AccountsSettingsPane sender, AccountsSettingsPaneCommandsRequestedEventArgs args)
        {
            // In order to make async calls within this callback, the deferral object is needed
            AccountsSettingsPaneEventDeferral deferral = args.GetDeferral();
            await AddWebAccountProvider(args);
            //AddLinksAndDescription(args);

            deferral.Complete();

        }

        const string MicrosoftAccountProviderId = "https://login.microsoft.com";
        const string ConsumerAuthority = "consumers";
        const string AccountScopeRequested = "User.Read"; // user.read is a graph scope. this ensure the token works for graph.
        const string AccountClientId = ""; // NeedToUseAppSettingsForThis

        private async Task AddWebAccountProvider(AccountsSettingsPaneCommandsRequestedEventArgs args)
        {
            WebAccountProvider provider = await WebAuthenticationCoreManager.FindAccountProviderAsync(MicrosoftAccountProviderId, ConsumerAuthority);

            WebAccountProviderCommand providerCommand = new WebAccountProviderCommand(provider, WebAccountProviderCommandInvoked);
            args.WebAccountProviderCommands.Add(providerCommand);
        }

        private async void WebAccountProviderCommandInvoked(WebAccountProviderCommand command)
        {
            // ClientID is ignored by MSA
            await RequestTokenAndSaveAccount(command.WebAccountProvider, AccountScopeRequested, AccountClientId);
        }

        private async Task RequestTokenAndSaveAccount(WebAccountProvider Provider, string Scope, string ClientID)
        {
            try
            {
                WebTokenRequest webTokenRequest = new WebTokenRequest(Provider, Scope, ClientID);
                //webTokenRequest.Properties.Add("resource", "https://graph.microsoft.com");

                // If the user selected a specific account, RequestTokenAsync will return a token for that account.
                // The user may be prompted for credentials or to authorize using that account with your app
                // If the user selected a provider, the user will be prompted for credentials to login to a new account
                WebTokenRequestResult webTokenRequestResult = await WebAuthenticationCoreManager.RequestTokenAsync(webTokenRequest);

                // If a token was successfully returned, then store the WebAccount Id into local app data
                // This Id can be used to retrieve the account whenever needed. To later get a token with that account
                // First retrieve the account with FindAccountAsync, and include that webaccount 
                // as a parameter to RequestTokenAsync or RequestTokenSilentlyAsync
                if (webTokenRequestResult.ResponseStatus == WebTokenRequestStatus.Success)
                {
                    //ApplicationData.Current.LocalSettings.Values.Remove(StoredAccountKey);

                    //ApplicationData.Current.LocalSettings.Values[StoredAccountKey] = webTokenRequestResult.ResponseData[0].WebAccount.Id;

                    var name = webTokenRequestResult.ResponseData[0].WebAccount.UserName;
                    var token = webTokenRequestResult.ResponseData[0].Token;
                    var picture = await webTokenRequestResult.ResponseData[0].WebAccount.GetPictureAsync(WebAccountPictureSize.Size64x64);
                }


                //OutputTokenResult(webTokenRequestResult);
            }
            catch (Exception ex)
            {
                //rootPage.NotifyUser("Web Token request failed: " + ex.Message, NotifyType.ErrorMessage);
            }
        }
    }
}
