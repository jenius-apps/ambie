using AmbientSounds.Animations;
using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
            {
                { "name", "catalogue" }
            });
        }
    }
}
