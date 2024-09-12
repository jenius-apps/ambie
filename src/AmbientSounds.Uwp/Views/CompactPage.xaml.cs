using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class CompactPage : Page
{
    private static readonly Dictionary<string, string> CommonTelemetryContent = new()
    {
        { "page", "compact" }
    };

    private readonly IUserSettings _userSettings;
    private CoreApplicationViewTitleBar? _coreTitleBar;

    public CompactPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<CompactPageViewModel>();
        _userSettings = App.Services.GetRequiredService<IUserSettings>();
    }

    public CompactPageViewModel ViewModel => (CompactPageViewModel)this.DataContext;

    private string BackgroundImagePath => _userSettings.Get<string>(UserSettingsConstants.BackgroundImage) ?? "http://localhost";

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        App.Services.GetRequiredService<ICompactNavigator>().ContentFrame = CompactContentFrame;

        // Required for interactive elements in bar.
        // Ref: https://docs.microsoft.com/en-us/windows/uwp/design/shell/title-bar#interactive-content
        _coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
        _coreTitleBar.LayoutMetricsChanged += TitleBarLayoutMetricsChanged;
        Window.Current.SetTitleBar(AppTitleBar);

        var telemetry = App.Services.GetRequiredService<ITelemetry>();
        telemetry.TrackPageView(nameof(CompactPage));

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

        // Why blank out this frame?
        // This is a workaround to ensure the uninitialize code
        // of the frame page is executed. 
        // This is required to avoid bugs with flipview index being changed
        // while the flipview is being populated.
        CompactContentFrame.Navigate(typeof(BlankPage));
    }

    private void UpdateBackgroundState()
    {
        bool backgroundImageActive = !string.IsNullOrEmpty(BackgroundImagePath);
        if (backgroundImageActive)
        {
            //FindName(nameof(BackgroundImage));
        }
    }

    private void TitleBarLayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
    {
        // Required for interactive elements in bar.
        // Ref: https://docs.microsoft.com/en-us/windows/uwp/design/shell/title-bar#interactive-content
        AppTitleBar.Height = sender.Height;
    }
}
