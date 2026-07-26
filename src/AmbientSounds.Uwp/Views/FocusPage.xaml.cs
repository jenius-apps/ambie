using AmbientSounds.Controls;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
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

        public bool MessageVisible => _tabletStyleDevices.Contains(AnalyticsInfo.DeviceForm);

        public FocusPageViewModel ViewModel => (FocusPageViewModel)this.DataContext;

        private bool IsDesktop => App.IsDesktop;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackPageView(nameof(FocusPage));

            var mainTask = ViewModel.InitializeAsync();
            await Task.WhenAll(_controlsToInitialize.Select(static x => x.InitializeAsync()));
            await mainTask;
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
