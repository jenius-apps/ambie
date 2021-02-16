using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace AmbientSounds.Controls
{
    public sealed partial class AccountControl : UserControl
    {
        public AccountControl()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<AccountControlViewModel>();
        }

        public AccountControlViewModel ViewModel => (AccountControlViewModel)this.DataContext;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AccountsSettingsPane.Show();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AccountsSettingsPane.GetForCurrentView().AccountCommandsRequested += OnAccountCommandsRequested;
            ViewModel.Load();
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
        //const string AccountScopeRequested = "wl.basic"; // wl.basic is a Live API scope used to get the user's profile picture.
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
                    ViewModel.SignedIn = true;
                    ViewModel.FullName = name;

                    var u = webTokenRequestResult.ResponseData[0].WebAccount.WebAccountProvider.User;
                    var streamReference = await u.GetPictureAsync(UserPictureSize.Size64x64);
                    if (streamReference != null)
                    {
                        IRandomAccessStream stream = await streamReference.OpenReadAsync();
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(stream);
                        Avatar.ProfilePicture = bitmapImage;
                    }
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
