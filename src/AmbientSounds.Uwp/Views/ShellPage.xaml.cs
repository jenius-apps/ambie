using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Services.Store;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

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

        // Set custom title bar.
        // Source: https://learn.microsoft.com/en-us/windows/apps/develop/title-bar?tabs=winui2#interactive-content 
        Window.Current.SetTitleBar(AppTitleBar);
    }

    public ShellPageViewModel ViewModel => (ShellPageViewModel)this.DataContext;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel.PropertyChanged += OnViewModelPropertyChanged;
        var navigator = App.Services.GetRequiredService<INavigator>();

        if (navigator.Frame is null)
        {
            navigator.Frame = MainFrame;

            if (e.NavigationMode != NavigationMode.Back &&
                e.Parameter is ShellPageNavigationArgs args)
            {
                ViewModel.Navigate(
                    args.FirstPageOverride ?? ContentPageType.Home,
                    args.LaunchArguments);
            }
        }

        await ViewModel.InitializeAsync();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        App.Services.GetRequiredService<INavigator>().Frame = null;
        ViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        ViewModel.Uninitialize();
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

    private async void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.FocusTimeBannerVisible))
        {
            if (ViewModel.FocusTimeBannerVisible)
            {
                await ShowTimeBannerAnimations.StartAsync();
            }
        }
        else if (e.PropertyName == nameof(ViewModel.GuideBannerVisible))
        {
            if (ViewModel.GuideBannerVisible)
            {
                await ShowGuideBannerAnimations.StartAsync();
            }
        }
    }
}
