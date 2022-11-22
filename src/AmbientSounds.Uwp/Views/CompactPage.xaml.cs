using AmbientSounds.Constants;
using AmbientSounds.Controls;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CompactPage : Page
{
    private readonly IUserSettings _userSettings;
    private CoreApplicationViewTitleBar? _coreTitleBar;

    public CompactPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<CompactPageViewModel>();
        _userSettings = App.Services.GetRequiredService<IUserSettings>();
    }

    public CompactPageViewModel ViewModel => (CompactPageViewModel)this.DataContext;

    private string BackgroundImagePath => _userSettings.Get<string>(UserSettingsConstants.BackgroundImage);

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        // Required for interactive elements in bar.
        // Ref: https://docs.microsoft.com/en-us/windows/uwp/design/shell/title-bar#interactive-content
        _coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
        _coreTitleBar.LayoutMetricsChanged += TitleBarLayoutMetricsChanged;
        Window.Current.SetTitleBar(AppTitleBar);

        var telemetry = App.Services.GetRequiredService<ITelemetry>();
        telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
        {
            { "name", "compact" }
        });

        if (e.Parameter is CompactViewMode requestedViewMode)
        {
            await ViewModel.InitializeAsync(requestedViewMode);
        }

        UpdateBackgroundState();
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        if (_coreTitleBar is { } bar)
        {
            bar.LayoutMetricsChanged -= TitleBarLayoutMetricsChanged;

            // Important! Need to unset the title bar so that the
            // main window's title bar works normally. If we do not unset this,
            // the main window's title bar does not support drag and drop.
            Window.Current.SetTitleBar(null);
        }
    }

    private void UpdateBackgroundState()
    {
        bool backgroundImageActive = !string.IsNullOrEmpty(BackgroundImagePath);
        if (backgroundImageActive)
        {
            FindName(nameof(BackgroundImage));
        }
    }

    private void TitleBarLayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
    {
        // Required for interactive elements in bar.
        // Ref: https://docs.microsoft.com/en-us/windows/uwp/design/shell/title-bar#interactive-content
        AppTitleBar.Height = sender.Height;
    }

    private void OnSegmentClicked(object sender, RoutedEventArgs e)
    {
        if (sender is SegmentItem item && item.Tag is string tag)
        {
            if (item.IsChecked)
            {
                // if item is already selected, do nothing.
                return;
            }

            switch (tag)
            {
                case "home":
                    ViewModel.CurrentView = CompactViewMode.Home;
                    break;
                case "focus":
                    ViewModel.CurrentView = CompactViewMode.Focus;
                    break;
            }
        }
    }
}
