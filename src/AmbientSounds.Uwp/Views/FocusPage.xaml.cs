using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using AmbientSounds.Constants;
using System.Collections.Generic;
using System.Threading.Tasks;
using AmbientSounds.Controls;
using System.Linq;
using JeniusApps.Common.Telemetry;

namespace AmbientSounds.Views
{
    public sealed partial class FocusPage : Page
    {
        private readonly ICanInitialize[] _controlsToInitialize;

        public FocusPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<FocusPageViewModel>();
            _controlsToInitialize = new ICanInitialize[]
            {
                TimerModule,
                HistoryModule,
                TaskModule
            };
        }

        public FocusPageViewModel ViewModel => (FocusPageViewModel)this.DataContext;

        private bool IsDesktop => App.IsDesktop;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackPageView(nameof(FocusPage));

            var mainTask = ViewModel.InitializeAsync();
            await Task.WhenAll(_controlsToInitialize.Select(x => x.InitializeAsync()));
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
