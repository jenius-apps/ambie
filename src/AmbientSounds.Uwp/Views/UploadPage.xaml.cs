using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UploadPage : Page
    {
        public UploadPage()
        {
            this.InitializeComponent();
        }

        private void GoBack()
        {
            if (App.AppFrame.CanGoBack)
            {
                App.AppFrame.GoBack();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = true;
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = false;
        }

        private void RefreshClicked(object sender, RoutedEventArgs e)
        {
            MySoundsList.Refresh();
        }
    }
}
