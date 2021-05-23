using System;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    // Source: Timo Partl
    // https://github.com/AzureAD/microsoft-authentication-library-for-dotnet/issues/1901#issuecomment-700596294

    public sealed partial class AuthDialog : ContentDialog
	{
		TaskCompletionSource<Uri> result = new();
		Uri authUri;
		string callbackUri;

		public AuthDialog(Uri uri, string callback)
		{
			InitializeComponent();
			authUri = uri;
			callbackUri = callback;
		}

		void ContentDialog_Loaded(object sender, RoutedEventArgs e) => webView.Navigate(authUri);

		void ContentDialog_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			webView.Height = Math.Min(756, e.NewSize.Height - 60);
			webView.Width = e.NewSize.Width;
		}

		public async Task<Uri> AuthenticateAsync()
		{
			await this.ShowAsync();
			return await result.Task;
		}

		void WebView_UnsupportedUriSchemeIdentified(WebView sender, WebViewUnsupportedUriSchemeIdentifiedEventArgs args)
		{
			if (args.Uri.AbsoluteUri.StartsWith(callbackUri, StringComparison.OrdinalIgnoreCase))
			{
				args.Handled = true;
				result.TrySetResult(args.Uri);
				Hide();
			}
		}

		void WebView_NavigationStarting(object sender, WebViewNavigationStartingEventArgs e)
		{
			if (e.Uri.AbsoluteUri.StartsWith(callbackUri, StringComparison.OrdinalIgnoreCase))
			{
				e.Cancel = true;
				result.TrySetResult(e.Uri);
				Hide();
			}
		}

		void BackBtn_Click(object sender, RoutedEventArgs e) => webView.GoBack();

		void CloseBtn_Click(object sender, RoutedEventArgs e)
		{
			result.TrySetException(new OperationCanceledException());
			Hide();
		}

		void WebView_NavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
		{
			result.TrySetException(new Exception(e.WebErrorStatus.ToString()));
			Hide();
		}
    }
}