using AmbientSounds.Controls;
using Microsoft.Identity.Client.Extensibility;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;

namespace AmbientSounds.Services.Uwp
{
    public class CustomAuthUiService : ICustomWebUi
    {
        public async Task<Uri> AcquireAuthorizationCodeAsync(
            Uri authorizationUri,
            Uri redirectUri,
            CancellationToken cancellationToken)
        {
            Uri result = await CoreApplication.MainView.CoreWindow.Dispatcher.RunTaskAsync(async () =>
            {
                var authDialog = new AuthDialog(authorizationUri, redirectUri.AbsoluteUri);
                return await authDialog.AuthenticateAsync();
            });

            return result;
        }
    }
}
