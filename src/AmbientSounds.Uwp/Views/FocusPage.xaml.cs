using AmbientSounds.Constants;
using AmbientSounds.Controls;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.System.Profile;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AmbientSounds.Views
{
    public sealed partial class FocusPage : Page
    {
        private readonly ICanInitialize[] _controlsToInitialize;
        private static readonly IReadOnlyList<string> _tabletStyleDevices = ["Detachable", "Convertible", "Tablet"];

        public static readonly DependencyProperty MessageVisibleProperty = DependencyProperty.Register(
            nameof(MessageVisible),
            typeof(bool),
            typeof(FocusPage),
            new PropertyMetadata(false));

        public FocusPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<FocusPageViewModel>();
            _controlsToInitialize =
            [
                TimerModule,
                HistoryModule,
                TaskModule
            ];
        }

        public bool MessageVisible
        {
            get => (bool)GetValue(MessageVisibleProperty);
            set => SetValue(MessageVisibleProperty, value);
        }

        public FocusPageViewModel ViewModel => (FocusPageViewModel)this.DataContext;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackPageView(nameof(FocusPage));

            var mainTask = ViewModel.InitializeAsync();
            await Task.WhenAll(_controlsToInitialize.Select(static x => x.InitializeAsync()));
            await mainTask;

            _ = UpdateMessageVisibleAsync();
        }

        private async Task UpdateMessageVisibleAsync()
        {
            if (_tabletStyleDevices.Contains(AnalyticsInfo.DeviceForm))
            {
                bool isAlreadyInstalled = await App.Services.GetRequiredService<ISystemInfoProvider>().IsAppInstalledAsync(AppConstants.MarkerpadPfn);
                MessageVisible = !isAlreadyInstalled;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.Uninitialize();
            foreach (var control in _controlsToInitialize)
            {
                control.Uninitialize();
            }
        }

        private async void OnUnloaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.SaveNotesToStorageAsync();
        }
    }
}
