using System;
using System.ComponentModel;
using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.ApplicationModel.Resources;
using Windows.Services.Store;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AmbientSounds.Views
{
    /// <summary>
    /// The root frame used to power the backgrounds of the app.
    /// </summary>
    public sealed partial class ShellPage : Page
    {
        public ShellPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<ShellPageViewModel>();
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            this.Unloaded += (_, _) => 
            {
                ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
                ViewModel.Dispose();
            };

            if (App.IsTenFoot)
            {
                // Ref: https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/turn-off-overscan
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            }
        }

        private async void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewModel.FocusTimeBannerVisibile))
            {
                if (ViewModel.FocusTimeBannerVisibile)
                {
                    await ShowTimeBannerAnimations.StartAsync();
                }
            }
        }

        public ShellPageViewModel ViewModel => (ShellPageViewModel)this.DataContext;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var navigator = App.Services.GetRequiredService<INavigator>();
            navigator.Frame = MainFrame;

            MenuList.SelectedIndex = 0;

            await ViewModel.InitializeAsync(e.Parameter as ShellPageNavigationArgs);
        }

        private async void TeachingTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            ViewModel.IsRatingMessageVisible = false;
            var storeContext = StoreContext.GetDefault();
            await storeContext.RequestRateAndReviewAppAsync();
            App.Services.GetRequiredService<IUserSettings>().Set(
                UserSettingsConstants.HasRated,
                true);
            App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.OobeRateUsClicked);
        }

        private void TeachingTip_CloseButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            ViewModel.IsRatingMessageVisible = false;
            App.Services.GetRequiredService<IUserSettings>().SetAndSerialize(
                UserSettingsConstants.RatingDismissed,
                DateTime.UtcNow,
                AmbieJsonSerializerContext.Default.DateTime);
            App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.OobeRateUsDismissed);
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView l)
            {
                ViewModel.Navigate(l.SelectedIndex);
            }
        }
    }
}
