using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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

        private bool ShowBackButton => !App.IsTenFoot;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var settings = App.Services.GetRequiredService<IUserSettings>();
            bool useDarkScreensaver = settings.Get<bool>(UserSettingsConstants.DarkScreensasver);
            if (!useDarkScreensaver)
            {
                FindName(nameof(ScreensaverControl));
            }

            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
            {
                { "name", "screensaver" },
                { "darkscreensaver", useDarkScreensaver ? "true" : "false" }
            });
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
