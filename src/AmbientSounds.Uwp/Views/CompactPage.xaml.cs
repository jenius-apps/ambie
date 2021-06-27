using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompactPage : Page
    {
        private readonly IUserSettings _userSettings;

        public CompactPage()
        {
            this.InitializeComponent();
            _userSettings = App.Services.GetRequiredService<IUserSettings>();
        }

        private string BackgroundImagePath => _userSettings.Get<string>(UserSettingsConstants.BackgroundImage);

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
            {
                { "name", "compact" }
            });

            UpdateBackgroundState();
        }

        private void UpdateBackgroundState()
        {
            bool backgroundImageActive = !string.IsNullOrEmpty(BackgroundImagePath);
            if (backgroundImageActive)
            {
                FindName(nameof(BackgroundImage));
            }
        }

        private async void CloseCompactClicked(object sender, RoutedEventArgs e)
        {
            var preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.Default);
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default, preferences);
            var navigator = App.Services.GetRequiredService<INavigator>();
            navigator.GoBack();
        }
    }
}
