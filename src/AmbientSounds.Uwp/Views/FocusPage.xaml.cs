using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using AmbientSounds.Constants;
using System.Collections.Generic;
using AmbientSounds.Models;
using AmbientSounds.Services;

namespace AmbientSounds.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FocusPage : Page
    {
        public FocusPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<FocusPageViewModel>();
        }

        public FocusPageViewModel ViewModel => (FocusPageViewModel)this.DataContext;

        private bool IsDesktop => App.IsDesktop;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var telemetry = App.Services.GetRequiredService<Services.ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
            {
                { "name", "focus" }
            });

            await ViewModel.InitializeAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            CloseAll();
            ViewModel.Uninitialize();
        }

        private void OnResetClicked(object sender, RoutedEventArgs e)
        {
            StartButton.Focus(Windows.UI.Xaml.FocusState.Programmatic);
            ViewModel.Stop();
        }

        private void OnRecentClicked(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is RecentFocusSettings s)
            {
                ViewModel.LoadRecentSettings(s);
            }
        }

        private void StartTutorial(object sender, RoutedEventArgs e)
        {
            if (!ViewModel.CanStartTutorial())
            {
                CloseAll();
                return;
            }

            ViewModel.IsHelpMessageVisible = false;
            TeachingTip1.IsOpen = true;
            TeachingTip2.IsOpen = false;
            TeachingTip3.IsOpen = false;
            TeachingTip4.IsOpen = false;

            App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.FocusTutorialStarted);
        }

        private void ShowTip2(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            if (!ViewModel.CanStartTutorial())
            {
                CloseAll();
                return;
            }

            TeachingTip1.IsOpen = false;
            TeachingTip2.IsOpen = true;
            TeachingTip3.IsOpen = false;
            TeachingTip4.IsOpen = false;
        }

        private void ShowTip3(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            if (!ViewModel.CanStartTutorial())
            {
                CloseAll();
                return;
            }

            TeachingTip1.IsOpen = false;
            TeachingTip2.IsOpen = false;
            TeachingTip3.IsOpen = true;
            TeachingTip4.IsOpen = false;
        }

        private void ShowTip4(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            if (!ViewModel.CanStartTutorial())
            {
                CloseAll();
                return;
            }

            TeachingTip1.IsOpen = false;
            TeachingTip2.IsOpen = false;
            TeachingTip3.IsOpen = false;
            TeachingTip4.IsOpen = true;
        }

        private void OnTutorialEnded(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            CloseAll();
            App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.FocusTutorialEnded);
        }

        private void CloseAll()
        {
            TeachingTip1.IsOpen = false;
            TeachingTip2.IsOpen = false;
            TeachingTip3.IsOpen = false;
            TeachingTip4.IsOpen = false;
        }

        private async void OnUnloaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.SaveNotesToStorageAsync();
        }
    }
}
