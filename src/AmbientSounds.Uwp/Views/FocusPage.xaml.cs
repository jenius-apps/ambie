using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using System.Threading.Tasks;
using System;
using Windows.UI.Xaml.Navigation;
using AmbientSounds.Constants;
using System.Collections.Generic;
using Windows.UI.Xaml.Automation;
using AmbientSounds.Models;

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

        private void OnResetClicked(object sender, RoutedEventArgs e)
        {
            StartButton.Focus(FocusState.Programmatic);
            ViewModel.Stop();
        }

        private void OnRecentClicked(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is RecentFocusSettings s)
            {
                ViewModel.LoadRecentSettings(s);
            }
        }
    }
}
