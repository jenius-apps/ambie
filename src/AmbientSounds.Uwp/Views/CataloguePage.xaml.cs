using AmbientSounds.Constants;
using AmbientSounds.Controls;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using WinUI = Microsoft.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Views
{
    public sealed partial class CataloguePage : Page
    {
        public CataloguePage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<CataloguePageViewModel>();
        }

        public CataloguePageViewModel ViewModel => (CataloguePageViewModel)this.DataContext;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var initializeTask = ViewModel.InitializeAsync();

            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
            {
                { "name", "catalogue" }
            });

            await initializeTask;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.Uninitialize();
        }
    }
}
