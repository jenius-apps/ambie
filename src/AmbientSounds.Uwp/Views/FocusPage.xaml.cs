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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.PropertyChanged += OnPropertyChanged;

            var telemetry = App.Services.GetRequiredService<Services.ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
            {
                { "name", "focus" }
            });
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.PropertyChanged -= OnPropertyChanged;
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.PlayVisible) && ViewModel.PlayVisible)
            {
                await Task.Delay(1);
                StartButton.Focus(FocusState.Programmatic);
            }
            else if (e.PropertyName == nameof(ViewModel.PauseVisible) && ViewModel.PauseVisible)
            {
                await Task.Delay(1);
                PauseButton.Focus(FocusState.Programmatic);
            }
        }
    }
}
