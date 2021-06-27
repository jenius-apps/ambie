using AmbientSounds.Animations;
using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views
{
    public sealed partial class CataloguePage : Page
    {
        public CataloguePage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<CataloguePageViewModel>();
        }

        public CataloguePageViewModel ViewModel => (CataloguePageViewModel)this.DataContext;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            UpdateBackgroundState();

            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
            {
                { "name", "catalogue" }
            });

            TryStartPageAnimations();

            var navigator = SystemNavigationManager.GetForCurrentView();
            navigator.BackRequested += OnBackRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            var navigator = SystemNavigationManager.GetForCurrentView();
            navigator.BackRequested -= OnBackRequested;
        }

        private void GoBack(object sender, RoutedEventArgs e)
        {
            var animation = ConnectedAnimationService
                .GetForCurrentView()
                .PrepareToAnimate(AnimationConstants.CatalogueBack, CatalogueBackplate);
                animation.Configuration = new DirectConnectedAnimationConfiguration();

            ViewModel.GoBack();
        }

        private void UpdateBackgroundState()
        {
            var userSettings = App.Services.GetRequiredService<IUserSettings>();
            bool backgroundImageActive = !string.IsNullOrEmpty(userSettings.Get<string>(UserSettingsConstants.BackgroundImage));
            VisualStateManager.GoToState(
                this,
                backgroundImageActive ? nameof(ImageBackgroundState) : nameof(RegularBackgroundState),
                false);
        }

        private void TryStartPageAnimations()
        {
            var animation = ConnectedAnimationService
                .GetForCurrentView()
                .GetAnimation(AnimationConstants.CatalogueForward);

            if (animation is not null)
            {
                animation.TryStart(CatalogueBackplate);
            }
        }

        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            ViewModel.GoBack();
            e.Handled = true;
        }
    }
}
