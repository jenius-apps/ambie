using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
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
            this.DataContext = App.Services.GetRequiredService<CompactPageViewModel>();
            _userSettings = App.Services.GetRequiredService<IUserSettings>();
        }

        public CompactPageViewModel ViewModel => (CompactPageViewModel)this.DataContext;

        private string BackgroundImagePath => _userSettings.Get<string>(UserSettingsConstants.BackgroundImage);

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
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

        private void UpdateBackgroundState()
        {
            bool backgroundImageActive = !string.IsNullOrEmpty(BackgroundImagePath);
            if (backgroundImageActive)
            {
                FindName(nameof(BackgroundImage));
            }
        }
    }
}
