using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views
{
    public sealed partial class ScreensaverPage : Page
    {
        public ScreensaverPage()
        {
            this.InitializeComponent();
        }

        private bool ShowBackButton => !App.IsTenFoot;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var settings = App.Services.GetRequiredService<IUserSettings>();
            bool useDarkScreensaver = settings.Get<bool>(UserSettingsConstants.DarkScreensasver);
            if (useDarkScreensaver)
            {
                VisualStateManager.GoToState(this, nameof(DarkScreensaverState), false);
            }
            else
            {
                FindName(nameof(ScreensaverControl));
            }

            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
            {
                { "name", "screensaver" },
                { "darkscreensaver", useDarkScreensaver ? "true" : "false" }
            });

            var coreWindow = CoreWindow.GetForCurrentThread();
            coreWindow.KeyDown += CataloguePage_KeyDown;
            var navigator = SystemNavigationManager.GetForCurrentView();
            navigator.BackRequested += OnBackRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var coreWindow = CoreWindow.GetForCurrentThread();
            coreWindow.KeyDown -= CataloguePage_KeyDown;
            var navigator = SystemNavigationManager.GetForCurrentView();
            navigator.BackRequested -= OnBackRequested;
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
                GoBack();
                args.Handled = true;
            }
        }

        private void GoBack()
        {
            var navigator = App.Services.GetRequiredService<INavigator>();
            navigator.GoBack(nameof(ScreensaverPage));
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            GoBack();
        }
    }
}
