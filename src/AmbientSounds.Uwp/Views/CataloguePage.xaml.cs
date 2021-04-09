using AmbientSounds.Animations;
using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AmbientSounds.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CataloguePage : Page
    {
        public CataloguePage()
        {
            this.InitializeComponent();
            var coreWindow = CoreWindow.GetForCurrentThread();
            coreWindow.KeyDown -= CataloguePage_KeyDown;
            coreWindow.KeyDown += CataloguePage_KeyDown;

            var navigator = SystemNavigationManager.GetForCurrentView();
            navigator.BackRequested -= OnBackRequested;
            navigator.BackRequested += OnBackRequested;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
            {
                { "name", "catalogue" }
            });

            TryStartPageAnimations();

            if (App.Services.GetRequiredService<IUserSettings>().Get<bool>(UserSettingsConstants.PerformanceMode))
            {
                SolidColorBrush mycolor = App.Current.Resources["FallbackBackgroundBrush"] as SolidColorBrush;
                if (mycolor != null)
                {
                    this.Background = mycolor;
                }
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ConnectedAnimationService
                .GetForCurrentView()
                .PrepareToAnimate(AnimationConstants.CatalogueBack, CatalogueIcon);
        }

        private void TryStartPageAnimations()
        {
            var animation = ConnectedAnimationService
                .GetForCurrentView()
                .GetAnimation(AnimationConstants.CatalogueForward);

            if (animation != null)
            {
                animation.TryStart(CatalogueIcon, new UIElement[] { CatalogueTitle });
            }
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (App.AppFrame.CanGoBack)
            {
                e.Handled = true;
                App.AppFrame.GoBack();
            }
        }

        private void CataloguePage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            if (args.VirtualKey == VirtualKey.Escape)
            {
                if (App.AppFrame.CanGoBack)
                {
                    args.Handled = true;
                    App.AppFrame.GoBack();
                }
            }
        }

        private void GoBack()
        {
            if (App.AppFrame.CanGoBack)
            {
                App.AppFrame.GoBack();
            }
        }
    }
}
