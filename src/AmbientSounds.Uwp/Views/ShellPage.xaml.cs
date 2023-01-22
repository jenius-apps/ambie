using System;
using System.ComponentModel;
using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.Services.Store;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.Toolkit.Uwp.UI;
using System.Collections.Generic;

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
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            var navigator = App.Services.GetRequiredService<INavigator>();

            if (navigator.Frame is null)
            {
                navigator.Frame = MainFrame;

                if (e.NavigationMode != NavigationMode.Back)
                {
                    ViewModel.Navigate(ContentPageType.Home);
                }
            }

            await ViewModel.InitializeAsync(e.Parameter as ShellPageNavigationArgs);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.Services.GetRequiredService<INavigator>().Frame = null;
            ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
            ViewModel.Dispose();
        }

        private async void TeachingTip_ActionButtonClick(Microsoft.UI.Xaml.Controls.TeachingTip sender, object args)
        {
            ViewModel.IsRatingMessageVisible = false;
            var storeContext = StoreContext.GetDefault();
            var result = await storeContext.RequestRateAndReviewAppAsync();
            App.Services.GetRequiredService<IUserSettings>().Set(
                UserSettingsConstants.HasRated,
                true);
            App.Services.GetRequiredService<ITelemetry>().TrackEvent(
                TelemetryConstants.OobeRateUsClicked,
                new Dictionary<string, string>
                {
                    { "status", result.Status.ToString() }
                });
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

        private void OnMenuItemClicked(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is FrameworkElement f && f.FindParent<ListViewItem>() is { Tag: string tag })
            {
                switch (tag)
                {
                    case "focus":
                        ViewModel.Navigate(ContentPageType.Focus);
                        break;
                    case "catalogue":
                        ViewModel.Navigate(ContentPageType.Catalogue);
                        break;
                    case "home":
                        ViewModel.Navigate(ContentPageType.Home);
                        break;
                    case "settings":
                        ViewModel.Navigate(ContentPageType.Settings);
                        break;
                }
            }
        }
    }
}
