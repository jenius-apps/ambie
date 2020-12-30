using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Views
{
    public sealed partial class ScreensaverPage : Page
    {
        public ScreensaverPage()
        {
            this.InitializeComponent();
            var coreWindow = CoreWindow.GetForCurrentThread();
            coreWindow.KeyDown -= CataloguePage_KeyDown;
            coreWindow.KeyDown += CataloguePage_KeyDown;

            var navigator = SystemNavigationManager.GetForCurrentView();
            navigator.BackRequested -= OnBackRequested;
            navigator.BackRequested += OnBackRequested;
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = true;
            GoBack();
        }

        private void CataloguePage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Escape)
            {
                args.Handled = true;
                GoBack();
            }
        }

        private void GoBack()
        {
            if (App.AppFrame.CanGoBack)
            {
                App.AppFrame.GoBack();
            }
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode && !App.IsTenFoot)
            {
                view.ExitFullScreenMode();
            }
        }
    }
}
